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
            var products = await _productRepository.GetAllAsync(); // Lấy tất cả sản phẩm
            return View(products); // Truyền danh sách sản phẩm vào View
        }

        // Action Add: Hiển thị form thêm sản phẩm
        public async Task<IActionResult> Add()
        {
            ViewBag.Categories = await _categoryRepository.GetAllAsync(); // Lấy danh mục để hiển thị
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
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
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
            ViewBag.Categories = await _categoryRepository.GetAllAsync(); // Lấy danh mục
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
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
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
