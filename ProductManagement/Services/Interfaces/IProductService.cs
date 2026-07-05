using ProductManagement.Models;

namespace ProductManagement.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllActiveAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<(bool Success, string? Error)> CreateAsync(Product product);
        Task<(bool Success, string? Error)> UpdateAsync(Product product);
        Task<(bool Success, string? Error)> DeleteAsync(int id);
        Task<bool> SkuExistsAsync(string sku, int? excludeId = null);
    }
}