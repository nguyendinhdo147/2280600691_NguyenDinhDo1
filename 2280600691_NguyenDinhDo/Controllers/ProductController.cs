using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _2280600691_NguyenDinhDo.Models;
using _2280600691_NguyenDinhDo.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace _2280600691_NguyenDinhDo.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Display All Products
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                ViewBag.CartCount = await GetCartCount(); // Retrieve cart count
                return View(products);
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError(string.Empty, "Failed to load products.");
                return View(Enumerable.Empty<Product>());
            }
        }

        // Get Cart Item Count for Navbar
        private async Task<int> GetCartCount()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var cart = await _context.ShoppingCarts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
                return cart?.Items.Sum(i => i.Quantity) ?? 0;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving cart count: {ex.Message}");
                return 0;
            }
        }

        // Add Product (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to load categories.");
                return View();
            }
        }

        // Add Product (POST)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
                    return View(product);
                }

                // Save image if provided
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                await _productRepository.AddAsync(product);
                TempData["Message"] = "Product added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to add product.");
                return View(product);
            }
        }

        // Save Image Helper Method
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", image.FileName);
            try
            {
                using (var fileStream = new FileStream(savePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
                return $"/images/{image.FileName}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save image.", ex);
            }
        }

        // Display Product Details
        public async Task<IActionResult> Display(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null) return NotFound();

                return View(product);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to load product details.");
                return RedirectToAction("Error", "Home");
            }
        }

        // Update Product (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null) return NotFound();

                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to load product details for update.");
                return RedirectToAction("Error", "Home");
            }
        }

        // Update Product (POST)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl");

            try
            {
                if (!ModelState.IsValid || id != product.Id)
                {
                    ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
                    return View(product);
                }

                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null) return NotFound();

                // Update image if provided
                product.ImageUrl = imageUrl == null ? existingProduct.ImageUrl : await SaveImage(imageUrl);

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;

                await _productRepository.UpdateAsync(existingProduct);
                TempData["Message"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to update product.");
                return View(product);
            }
        }

        // Delete Product (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null) return NotFound();

                return View(product);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to load product for deletion.");
                return RedirectToAction("Error", "Home");
            }
        }

        // Delete Product (POST)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _productRepository.DeleteAsync(id);
                TempData["Message"] = "Product deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to delete product.");
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
