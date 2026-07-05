using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProductManagement.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<SelectListItem>> GetActiveCategorySelectListAsync();
    }
}