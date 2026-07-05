using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using ProductManagement.Models;
using ProductManagement.Services.Interfaces;

namespace ProductManagement.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllActiveAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
        {
            return await _context.Products
                .AnyAsync(p => p.SKU == sku && (excludeId == null || p.Id != excludeId));
        }

        public async Task<(bool Success, string? Error)> CreateAsync(Product product)
        {
            try
            {
                product.CreatedUtc = DateTime.UtcNow;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product.");
                return (false, "An error occurred while saving the product.");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(Product product)
        {
            try
            {
                var existing = await _context.Products.FindAsync(product.Id);
                if (existing == null)
                    return (false, "Product not found.");

                existing.Name = product.Name;
                existing.SKU = product.SKU;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.QuantityInStock = product.QuantityInStock;
                existing.CategoryId = product.CategoryId;
                existing.IsActive = product.IsActive;
                existing.UpdatedUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Products.AnyAsync(p => p.Id == product.Id))
                    return (false, "Product not found.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}.", product.Id);
                return (false, "An error occurred while updating the product.");
            }
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return (false, "Product not found.");

                _context.Products.Remove(product); // Hard delete (acceptable per requirements)
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}.", id);
                return (false, "Unable to delete product. It may be referenced elsewhere.");
            }
        }
    }
}