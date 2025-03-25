using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2280600691_NguyenDinhDo.Models;
using _2280600691_NguyenDinhDo.Repositories;
using _2280600691_NguyenDinhDo.Hubs;
using Microsoft.AspNetCore.SignalR;
using _2280600691_NguyenDinhDo.Extensions;

namespace _2280600691_NguyenDinhDo.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<CartHub> _hubContext;

        public ShoppingCartController(IProductRepository productRepository, UserManager<ApplicationUser> userManager, ApplicationDbContext context, IHubContext<CartHub> hubContext)
        {
            _productRepository = productRepository;
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult Checkout()
        {
            return View(new Order());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            if (!ModelState.IsValid)
            {
                // Trả lại view với lỗi nhập liệu
                return View(order);
            }

            var cart = await GetUserCartAsync();
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống...
                ModelState.AddModelError(string.Empty, "Your cart is empty. Please add items before checking out.");
                return View(order);
            }

            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Xóa giỏ hàng sau khi hoàn thành đơn hàng
            _context.ShoppingCarts.Remove(cart);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderCompleted", new { orderId = order.Id });
        }

        public IActionResult OrderCompleted(int orderId)
        {
            return View("~/Views/ShoppingCart/OrderCompleted.cshtml", orderId);
        }


        private async Task<ShoppingCart> GetUserCartAsync()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.ShoppingCarts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await GetUserCartAsync();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = await GetUserCartAsync();

            // Sử dụng khóa lock để tránh việc thêm lặp khi có nhiều request cùng lúc
            lock (cart)
            {
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = productId,
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = quantity
                    });
                }
            }

            _context.ShoppingCarts.Update(cart);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(_userManager.GetUserId(User)).SendAsync("UpdateCartCount", cart.Items.Sum(i => i.Quantity));

            return Json(new { success = true, cartCount = cart.Items.Sum(i => i.Quantity) });
        }

        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var cart = await GetUserCartAsync();

            // Tìm và xóa sản phẩm khỏi giỏ hàng
            var itemToRemove = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
                _context.ShoppingCarts.Update(cart);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
