using _2280600691_NguyenDinhDo.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace _2280600691_NguyenDinhDo.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        // Constructor: Đảm bảo _context không null
        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Lấy danh sách sản phẩm
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category) // Bao gồm thông tin Category
                    .ToListAsync();

                // Kiểm tra xem có sản phẩm hay không
                if (products == null || !products.Any())
                {
                    Console.WriteLine("No products found in the database.");
                }

                return products ?? new List<Product>(); // Đảm bảo không trả về null
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
                return new List<Product>(); // Trả về danh sách rỗng nếu có lỗi
            }
        }

        // Lấy sản phẩm theo ID
        public async Task<Product> GetByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category) // Bao gồm thông tin Category
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                }

                return product;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Error in GetByIdAsync: {ex.Message}");
                return null; // Trả về null nếu xảy ra lỗi
            }
        }

        // Thêm sản phẩm
        public async Task AddAsync(Product product)
        {
            try
            {
                if (product == null)
                {
                    throw new ArgumentNullException(nameof(product), "Product cannot be null.");
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Error in AddAsync: {ex.Message}");
                throw; // Tiếp tục ném ngoại lệ để controller xử lý
            }
        }

        // Cập nhật sản phẩm
        public async Task UpdateAsync(Product product)
        {
            try
            {
                if (product == null)
                {
                    throw new ArgumentNullException(nameof(product), "Product cannot be null.");
                }

                var existingProduct = await _context.Products.FindAsync(product.Id);

                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"Product with ID {product.Id} not found.");
                }

                // Cập nhật các trường
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Error in UpdateAsync: {ex.Message}");
                throw;
            }
        }

        // Xóa sản phẩm
        public async Task DeleteAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found.");
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Error in DeleteAsync: {ex.Message}");
                throw;
            }
        }
    }
}
