using _2280600691_NguyenDinhDo.Areas.Admin.Models;
using _2280600691_NguyenDinhDo.Models;
using _2280600691_NguyenDinhDo.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _2280600691_NguyenDinhDo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // Chỉ cho Admin truy cập

    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        // Constructor để inject repositories
        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // Action Index: Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                if (products == null || !products.Any())
                {
                    ViewData["Message"] = "Không có sản phẩm nào để hiển thị.";
                }
                return View(products ?? Enumerable.Empty<Product>());
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tải sản phẩm.");
                return View(Enumerable.Empty<Product>());
            }
        }


        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories ?? Enumerable.Empty<Category>(); // Lấy danh mục để hiển thị
            return View();
        }

        // Xử lý thêm sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productRepository.AddAsync(product); // Thêm sản phẩm mới
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories ?? Enumerable.Empty<Category>();
            return View(product);
        }

        // Action Edit: Hiển thị form chỉnh sửa sản phẩm
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories ?? Enumerable.Empty<Category>(); // Lấy danh mục
            return View(product);
        }

        // Xử lý chỉnh sửa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productRepository.UpdateAsync(product); // Cập nhật sản phẩm
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories ?? Enumerable.Empty<Category>();
            return View(product);
        }

        // Action Delete: Xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id); // Xóa sản phẩm
            return RedirectToAction(nameof(Index));
        }
    }
}
