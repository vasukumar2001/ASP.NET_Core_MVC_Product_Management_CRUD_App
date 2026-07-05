using Microsoft.AspNetCore.Mvc;
using ProductManagement.Models;
using ProductManagement.Services.Interfaces;

namespace ProductManagement.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllActiveAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product list.");
                TempData["ErrorMessage"] = "Unable to load products. Please try again later.";
                return View(new List<Product>());
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetByIdAsync(id.Value);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            var vm = new ProductViewModel
            {
                CategoryList = await _categoryService.GetActiveCategorySelectListAsync()
            };
            return View(vm);
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel vm)
        {
            if (await _productService.SkuExistsAsync(vm.SKU))
            {
                ModelState.AddModelError(nameof(vm.SKU), "This SKU already exists.");
            }

            if (!ModelState.IsValid)
            {
                vm.CategoryList = await _categoryService.GetActiveCategorySelectListAsync();
                return View(vm);
            }

            var product = new Product
            {
                Name = vm.Name,
                SKU = vm.SKU,
                Description = vm.Description,
                Price = vm.Price,
                QuantityInStock = vm.QuantityInStock,
                CategoryId = vm.CategoryId,
                IsActive = vm.IsActive
            };

            var (success, error) = await _productService.CreateAsync(product);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "An error occurred while saving the product.");
                vm.CategoryList = await _categoryService.GetActiveCategorySelectListAsync();
                return View(vm);
            }

            TempData["SuccessMessage"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetByIdAsync(id.Value);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                QuantityInStock = product.QuantityInStock,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive,
                CategoryList = await _categoryService.GetActiveCategorySelectListAsync()
            };

            return View(vm);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (await _productService.SkuExistsAsync(vm.SKU, id))
            {
                ModelState.AddModelError(nameof(vm.SKU), "This SKU already exists.");
            }

            if (!ModelState.IsValid)
            {
                vm.CategoryList = await _categoryService.GetActiveCategorySelectListAsync();
                return View(vm);
            }

            var product = new Product
            {
                Id = vm.Id,
                Name = vm.Name,
                SKU = vm.SKU,
                Description = vm.Description,
                Price = vm.Price,
                QuantityInStock = vm.QuantityInStock,
                CategoryId = vm.CategoryId,
                IsActive = vm.IsActive
            };

            var (success, error) = await _productService.UpdateAsync(product);

            if (!success)
            {
                if (error == "Product not found.")
                {
                    TempData["ErrorMessage"] = error;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, error ?? "An error occurred while updating the product.");
                vm.CategoryList = await _categoryService.GetActiveCategorySelectListAsync();
                return View(vm);
            }

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetByIdAsync(id.Value);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, error) = await _productService.DeleteAsync(id);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Product deleted successfully." : error;

            return RedirectToAction(nameof(Index));
        }
    }
}