# Product Management — ASP.NET Core MVC (.NET 8) — Complete Guide

A step-by-step guide to build a Product Management CRUD app using ASP.NET Core MVC, EF Core, SQL Server, Bootstrap 5, and a clean **Interface + Service** layer (Controller → Service → DbContext).

---

## 1. Overview

**Goal:** Build a Product Management web app supporting: list, view details, create, edit, and delete products, backed by a SQL database with Categories and Products tables.

**Stack:** ASP.NET Core MVC · .NET 8 · Entity Framework Core · SQL Server · Razor Views · Bootstrap 5

**Architecture:** Controllers no longer talk to `DbContext` directly. Each entity has an interface (`IProductService`, `ICategoryService`) and a concrete implementation in `Services/`, registered via dependency injection. This keeps controllers thin, makes the data-access layer swappable/mockable, and matches standard layered-architecture practice.

---

## 2. Project Structure

```
Controllers/ProductsController.cs
Data/ApplicationDbContext.cs
Models/Category.cs
Models/Product.cs
Models/ProductViewModel.cs
ProductManagement.csproj
Program.cs
README.md
SQL/schema.sql
Services/CategoryService.cs
Services/Interfaces/ICategoryService.cs
Services/Interfaces/IProductService.cs
Services/ProductService.cs
Views/Products/Create.cshtml
Views/Products/Delete.cshtml
Views/Products/Details.cshtml
Views/Products/Edit.cshtml
Views/Products/Index.cshtml
Views/Shared/_Layout.cshtml
Views/Shared/_ValidationScriptsPartial.cshtml
Views/_ViewImports.cshtml
Views/_ViewStart.cshtml
appsettings.json
wwwroot/css/site.css
```

---

## 3. Step-by-Step Setup

### Step 1 — Create the project
```bash
dotnet new mvc -n ProductManagement
cd ProductManagement
```

### Step 2 — Add NuGet packages
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.10
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.10
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.10
```

### Step 3 — Add Models, DbContext, Services, Controller, Views
Copy the files below into the matching folders (create folders as needed):
`Models/`, `Data/`, `Services/Interfaces/`, `Services/`, `Controllers/`, `Views/`.

### Step 4 — Register the service layer in `Program.cs`
```csharp
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
```

### Step 5 — Set the connection string
Edit `appsettings.json` to point at your SQL Server / LocalDB instance.

### Step 6 — Create the database
Choose **one**:

**Option A — EF Core Migrations (recommended)**
```bash
dotnet tool install --global dotnet-ef   # if not already installed
Add-Migration InitialCreate
dotnet ef database update
```
This creates the schema and seeds data automatically via `HasData` in `ApplicationDbContext`.

**Option B — Manual SQL Script**
Run `SQL/schema.sql` (Section 10 below) in SSMS / Azure Data Studio. It creates the database, both tables with constraints, and seeds 3 categories + 8 products.

### Step 7 — Run the app
```bash
dotnet run
```
Browse to `https://localhost:5001` — it opens directly on `Products/Index`.

---

## 4. Architecture: Interface + Service Pattern

```
Controller  →  IProductService / ICategoryService  →  ProductService / CategoryService  →  ApplicationDbContext (EF Core)  →  SQL Server
```

- **`Services/Interfaces/IProductService.cs`** and **`ICategoryService.cs`** define the contract — what the controller is allowed to ask for, without knowing how it's fetched.
- **`Services/ProductService.cs`** and **`CategoryService.cs`** implement those contracts using EF Core (`async`/`await` throughout), and contain the business rules (duplicate SKU check, hard delete, timestamps).
- **`Controllers/ProductsController.cs`** only depends on the two interfaces (constructor injection) — it has no `DbSet`/`DbContext` reference at all.
- **`Program.cs`** wires the concrete classes to their interfaces with `AddScoped`, so ASP.NET Core's DI container supplies the right implementation at runtime (and a test project could substitute a fake/mock implementation with zero controller changes).

---

## 5. Database Schema

### Categories
| Column | Type | Constraints |
|---|---|---|
| Id | int | PK, Identity |
| Name | nvarchar(100) | Required, Unique |
| Description | nvarchar(250) | Nullable |
| IsActive | bit | Required, Default = 1 |
| CreatedUtc | datetime2 | Required |

### Products
| Column | Type | Constraints |
|---|---|---|
| Id | int | PK, Identity |
| Name | nvarchar(150) | Required |
| SKU | nvarchar(50) | Required, Unique |
| Description | nvarchar(500) | Nullable |
| Price | decimal(18,2) | Required, > 0 |
| QuantityInStock | int | Required, >= 0 |
| CategoryId | int | Required, FK → Categories.Id |
| IsActive | bit | Required, Default = 1 |
| CreatedUtc | datetime2 | Required |
| UpdatedUtc | datetime2 | Nullable |

---

## 6. Project File

**`ProductManagement.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.10">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.10">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

</Project>
```

**`appsettings.json`**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**`Program.cs`**
```csharp
using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using ProductManagement.Services;
using ProductManagement.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Service layer (Interface + Class pattern) - keeps controllers thin and data access testable
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
```

---

## 7. Models

**`Models/Category.cs`**
```csharp
using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
```

**`Models/Product.cs`**
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagement.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(150)]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required.")]
        [StringLength(50)]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be greater than 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater.")]
        [Display(Name = "Quantity In Stock")]
        public int QuantityInStock { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        // Navigation property
        public Category? Category { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedUtc { get; set; }
    }
}
```

**`Models/ProductViewModel.cs`**
```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProductManagement.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(150)]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required.")]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater.")]
        [Display(Name = "Quantity In Stock")]
        public int QuantityInStock { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Populated by controller for the dropdown
        public IEnumerable<SelectListItem>? CategoryList { get; set; }
    }
}
```

---

## 8. Data Layer

**`Data/ApplicationDbContext.cs`**
```csharp
using Microsoft.EntityFrameworkCore;
using ProductManagement.Models;

namespace ProductManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----- Category configuration -----
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
                entity.Property(c => c.IsActive).HasDefaultValue(true);
            });

            // ----- Product configuration -----
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.SKU).IsUnique();
                entity.Property(p => p.IsActive).HasDefaultValue(true);

                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ----- Seed Categories -----
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories", IsActive = true, CreatedUtc = new DateTime(2026, 1, 1) },
                new Category { Id = 2, Name = "Office Supplies", Description = "Office and stationery items", IsActive = true, CreatedUtc = new DateTime(2026, 1, 1) },
                new Category { Id = 3, Name = "Furniture", Description = "Home and office furniture", IsActive = true, CreatedUtc = new DateTime(2026, 1, 1) }
            );

            // ----- Seed Products (minimum 8) -----
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Wireless Mouse", SKU = "ELEC-001", Description = "Ergonomic wireless mouse", Price = 19.99m, QuantityInStock = 150, CategoryId = 1, IsActive = true, CreatedUtc = new DateTime(2026, 1, 2) },
                new Product { Id = 2, Name = "Mechanical Keyboard", SKU = "ELEC-002", Description = "RGB backlit mechanical keyboard", Price = 59.99m, QuantityInStock = 80, CategoryId = 1, IsActive = true, CreatedUtc = new DateTime(2026, 1, 2) },
                new Product { Id = 3, Name = "27-inch Monitor", SKU = "ELEC-003", Description = "Full HD IPS monitor", Price = 179.99m, QuantityInStock = 40, CategoryId = 1, IsActive = true, CreatedUtc = new DateTime(2026, 1, 2) },
                new Product { Id = 4, Name = "A4 Paper Ream", SKU = "OFF-001", Description = "500 sheets, 80gsm", Price = 4.99m, QuantityInStock = 500, CategoryId = 2, IsActive = true, CreatedUtc = new DateTime(2026, 1, 3) },
                new Product { Id = 5, Name = "Ballpoint Pen Pack", SKU = "OFF-002", Description = "Pack of 10 blue pens", Price = 3.49m, QuantityInStock = 300, CategoryId = 2, IsActive = true, CreatedUtc = new DateTime(2026, 1, 3) },
                new Product { Id = 6, Name = "Stapler", SKU = "OFF-003", Description = "Heavy duty stapler", Price = 8.99m, QuantityInStock = 120, CategoryId = 2, IsActive = true, CreatedUtc = new DateTime(2026, 1, 3) },
                new Product { Id = 7, Name = "Office Chair", SKU = "FURN-001", Description = "Ergonomic mesh office chair", Price = 149.99m, QuantityInStock = 25, CategoryId = 3, IsActive = true, CreatedUtc = new DateTime(2026, 1, 4) },
                new Product { Id = 8, Name = "Standing Desk", SKU = "FURN-002", Description = "Height-adjustable standing desk", Price = 299.99m, QuantityInStock = 15, CategoryId = 3, IsActive = true, CreatedUtc = new DateTime(2026, 1, 4) }
            );
        }
    }
}
```

---

## 9. Service Layer (Interface + Class pattern)

**`Services/Interfaces/IProductService.cs`**
```csharp
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
```

**`Services/Interfaces/ICategoryService.cs`**
```csharp
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProductManagement.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<SelectListItem>> GetActiveCategorySelectListAsync();
    }
}
```

**`Services/ProductService.cs`**
```csharp
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
```

**`Services/CategoryService.cs`**
```csharp
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using ProductManagement.Services.Interfaces;

namespace ProductManagement.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetActiveCategorySelectListAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
```

---

## 10. Controller

**`Controllers/ProductsController.cs`**
```csharp
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
```

---

## 11. SQL Script (`SQL/schema.sql`)

```sql
/* =========================================================
   Product Management Database - Schema & Seed Script
   Target: SQL Server
   ========================================================= */

IF DB_ID('ProductManagementDb') IS NULL
BEGIN
    CREATE DATABASE ProductManagementDb;
END
GO

USE ProductManagementDb;
GO

/* ---------------------------------------------------------
   1. Categories Table
   --------------------------------------------------------- */
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
GO

CREATE TABLE dbo.Categories
(
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(100)  NOT NULL,
    Description   NVARCHAR(250)  NULL,
    IsActive      BIT            NOT NULL DEFAULT (1),
    CreatedUtc    DATETIME2      NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT UQ_Categories_Name UNIQUE (Name)
);
GO

/* ---------------------------------------------------------
   2. Products Table
   --------------------------------------------------------- */
CREATE TABLE dbo.Products
(
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    Name              NVARCHAR(150)   NOT NULL,
    SKU               NVARCHAR(50)    NOT NULL,
    Description       NVARCHAR(500)   NULL,
    Price             DECIMAL(18,2)   NOT NULL,
    QuantityInStock   INT             NOT NULL,
    CategoryId        INT             NOT NULL,
    IsActive          BIT             NOT NULL DEFAULT (1),
    CreatedUtc        DATETIME2       NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedUtc        DATETIME2       NULL,

    CONSTRAINT UQ_Products_SKU UNIQUE (SKU),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Products_Price CHECK (Price > 0),
    CONSTRAINT CK_Products_Quantity CHECK (QuantityInStock >= 0)
);
GO

/* ---------------------------------------------------------
   3. Seed Data - Categories (minimum 3 required)
   --------------------------------------------------------- */
INSERT INTO dbo.Categories (Name, Description, IsActive, CreatedUtc)
VALUES
    ('Electronics',      'Electronic devices and accessories', 1, SYSUTCDATETIME()),
    ('Office Supplies',  'Office and stationery items',        1, SYSUTCDATETIME()),
    ('Furniture',        'Home and office furniture',          1, SYSUTCDATETIME());
GO

/* ---------------------------------------------------------
   4. Seed Data - Products (minimum 8 required)
   --------------------------------------------------------- */
INSERT INTO dbo.Products (Name, SKU, Description, Price, QuantityInStock, CategoryId, IsActive, CreatedUtc)
VALUES
    ('Wireless Mouse',        'ELEC-001', 'Ergonomic wireless mouse',          19.99,  150, 1, 1, SYSUTCDATETIME()),
    ('Mechanical Keyboard',   'ELEC-002', 'RGB backlit mechanical keyboard',   59.99,  80,  1, 1, SYSUTCDATETIME()),
    ('27-inch Monitor',       'ELEC-003', 'Full HD IPS monitor',               179.99, 40,  1, 1, SYSUTCDATETIME()),
    ('A4 Paper Ream',         'OFF-001',  '500 sheets, 80gsm',                 4.99,   500, 2, 1, SYSUTCDATETIME()),
    ('Ballpoint Pen Pack',    'OFF-002',  'Pack of 10 blue pens',              3.49,   300, 2, 1, SYSUTCDATETIME()),
    ('Stapler',               'OFF-003',  'Heavy duty stapler',                8.99,   120, 2, 1, SYSUTCDATETIME()),
    ('Office Chair',          'FURN-001', 'Ergonomic mesh office chair',       149.99, 25,  3, 1, SYSUTCDATETIME()),
    ('Standing Desk',         'FURN-002', 'Height-adjustable standing desk',   299.99, 15,  3, 1, SYSUTCDATETIME());
GO

/* ---------------------------------------------------------
   5. Verification queries (optional)
   --------------------------------------------------------- */
-- SELECT * FROM dbo.Categories;
-- SELECT p.*, c.Name AS CategoryName FROM dbo.Products p JOIN dbo.Categories c ON p.CategoryId = c.Id;
```

---

## 12. Razor Views (Bootstrap 5 UI)

**`Views/_ViewImports.cshtml`**
```cshtml
@using ProductManagement
@using ProductManagement.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

**`Views/_ViewStart.cshtml`**
```cshtml
@{
    Layout = "_Layout";
}
```

**`Views/Shared/_Layout.cshtml`**
```cshtml
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Product Management</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.3/css/bootstrap.min.css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    <link rel="stylesheet" href="~/css/site.css" />
</head>
<body class="bg-light">
    <header>
        <nav class="navbar navbar-expand-sm navbar-dark bg-dark mb-4 shadow-sm">
            <div class="container">
                <a class="navbar-brand fw-bold" asp-controller="Products" asp-action="Index">
                    <i class="fa-solid fa-boxes-stacked me-2"></i>Product Management
                </a>
                <div class="navbar-nav">
                    <a class="nav-link text-white" asp-controller="Products" asp-action="Index">Products</a>
                    <a class="nav-link text-white" asp-controller="Products" asp-action="Create">Add Product</a>
                </div>
            </div>
        </nav>
    </header>

    <div class="container">
        @if (TempData["SuccessMessage"] != null)
        {
            <div class="alert alert-success alert-dismissible fade show" role="alert">
                <i class="fa-solid fa-circle-check me-1"></i> @TempData["SuccessMessage"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @if (TempData["ErrorMessage"] != null)
        {
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fa-solid fa-triangle-exclamation me-1"></i> @TempData["ErrorMessage"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }

        <main role="main">
            @RenderBody()
        </main>
    </div>

    <footer class="border-top footer text-muted mt-5 py-3">
        <div class="container text-center">
            &copy; @DateTime.Now.Year - Product Management Application
        </div>
    </footer>

    <script src="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.3/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

**`Views/Shared/_ValidationScriptsPartial.cshtml`**
```cshtml
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.7.1/jquery.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-validate/1.20.1/jquery.validate.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-validation-unobtrusive/4.0.0/jquery.validate.unobtrusive.min.js"></script>
```

**`Views/Products/Index.cshtml`**
```cshtml
@model IEnumerable<Product>

@{
    ViewData["Title"] = "Products";
}

<div class="d-flex justify-content-between align-items-center mb-3">
    <h2 class="mb-0"><i class="fa-solid fa-box-open me-2 text-primary"></i>Products</h2>
    <a asp-action="Create" class="btn btn-primary">
        <i class="fa-solid fa-plus me-1"></i> Add New Product
    </a>
</div>

<div class="card shadow-sm">
    <div class="card-body p-0">
        <div class="table-responsive">
            <table class="table table-hover align-middle mb-0">
                <thead class="table-dark">
                    <tr>
                        <th>Product Name</th>
                        <th>SKU</th>
                        <th>Category</th>
                        <th class="text-end">Price</th>
                        <th class="text-end">Qty In Stock</th>
                        <th class="text-center">Active</th>
                        <th>Created Date</th>
                        <th class="text-center">Actions</th>
                    </tr>
                </thead>
                <tbody>
                @if (Model.Any())
                {
                    @foreach (var item in Model)
                    {
                        <tr>
                            <td class="fw-semibold">@item.Name</td>
                            <td><span class="badge bg-secondary">@item.SKU</span></td>
                            <td>@item.Category?.Name</td>
                            <td class="text-end">@item.Price.ToString("C")</td>
                            <td class="text-end">@item.QuantityInStock</td>
                            <td class="text-center">
                                @if (item.IsActive)
                                {
                                    <span class="badge bg-success">Yes</span>
                                }
                                else
                                {
                                    <span class="badge bg-danger">No</span>
                                }
                            </td>
                            <td>@item.CreatedUtc.ToString("dd MMM yyyy")</td>
                            <td class="text-center">
                                <div class="btn-group btn-group-sm">
                                    <a asp-action="Details" asp-route-id="@item.Id" class="btn btn-outline-info" title="View">
                                        <i class="fa-solid fa-eye"></i>
                                    </a>
                                    <a asp-action="Edit" asp-route-id="@item.Id" class="btn btn-outline-warning" title="Edit">
                                        <i class="fa-solid fa-pen"></i>
                                    </a>
                                    <a asp-action="Delete" asp-route-id="@item.Id" class="btn btn-outline-danger" title="Delete">
                                        <i class="fa-solid fa-trash"></i>
                                    </a>
                                </div>
                            </td>
                        </tr>
                    }
                }
                else
                {
                    <tr>
                        <td colspan="8" class="text-center text-muted py-4">No products found.</td>
                    </tr>
                }
                </tbody>
            </table>
        </div>
    </div>
</div>
```

**`Views/Products/Details.cshtml`**
```cshtml
@model Product

@{
    ViewData["Title"] = "Product Details";
}

<h2 class="mb-4"><i class="fa-solid fa-circle-info me-2 text-primary"></i>Product Details</h2>

<div class="card shadow-sm" style="max-width: 700px;">
    <div class="card-body">
        <dl class="row mb-0">
            <dt class="col-sm-4">Product Name</dt>
            <dd class="col-sm-8">@Model.Name</dd>

            <dt class="col-sm-4">SKU</dt>
            <dd class="col-sm-8"><span class="badge bg-secondary">@Model.SKU</span></dd>

            <dt class="col-sm-4">Category</dt>
            <dd class="col-sm-8">@Model.Category?.Name</dd>

            <dt class="col-sm-4">Description</dt>
            <dd class="col-sm-8">@(string.IsNullOrWhiteSpace(Model.Description) ? "—" : Model.Description)</dd>

            <dt class="col-sm-4">Price</dt>
            <dd class="col-sm-8">@Model.Price.ToString("C")</dd>

            <dt class="col-sm-4">Quantity In Stock</dt>
            <dd class="col-sm-8">@Model.QuantityInStock</dd>

            <dt class="col-sm-4">Is Active</dt>
            <dd class="col-sm-8">
                @if (Model.IsActive)
                {
                    <span class="badge bg-success">Active</span>
                }
                else
                {
                    <span class="badge bg-danger">Inactive</span>
                }
            </dd>

            <dt class="col-sm-4">Created Date</dt>
            <dd class="col-sm-8">@Model.CreatedUtc.ToString("dd MMM yyyy HH:mm")</dd>

            <dt class="col-sm-4">Last Updated</dt>
            <dd class="col-sm-8">@(Model.UpdatedUtc?.ToString("dd MMM yyyy HH:mm") ?? "—")</dd>
        </dl>
    </div>
    <div class="card-footer bg-white d-flex gap-2">
        <a asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-warning">
            <i class="fa-solid fa-pen me-1"></i> Edit
        </a>
        <a asp-action="Index" class="btn btn-outline-secondary">
            <i class="fa-solid fa-arrow-left me-1"></i> Back to List
        </a>
    </div>
</div>
```

**`Views/Products/Create.cshtml`**
```cshtml
@model ProductViewModel

@{
    ViewData["Title"] = "Create Product";
}

<h2 class="mb-4"><i class="fa-solid fa-plus me-2 text-primary"></i>Create Product</h2>

<div class="card shadow-sm" style="max-width: 700px;">
    <div class="card-body">
        <form asp-action="Create" method="post" novalidate>
            <div asp-validation-summary="ModelOnly" class="alert alert-danger d-none" data-val-summary></div>
            <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

            <div class="mb-3">
                <label asp-for="Name" class="form-label"></label>
                <input asp-for="Name" class="form-control" placeholder="e.g. Wireless Mouse" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="SKU" class="form-label"></label>
                <input asp-for="SKU" class="form-control" placeholder="e.g. ELEC-009" />
                <span asp-validation-for="SKU" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="Description" class="form-label"></label>
                <textarea asp-for="Description" class="form-control" rows="3"></textarea>
                <span asp-validation-for="Description" class="text-danger"></span>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="Price" class="form-label"></label>
                    <div class="input-group">
                        <span class="input-group-text">$</span>
                        <input asp-for="Price" class="form-control" type="number" step="0.01" min="0.01" />
                    </div>
                    <span asp-validation-for="Price" class="text-danger"></span>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="QuantityInStock" class="form-label"></label>
                    <input asp-for="QuantityInStock" class="form-control" type="number" min="0" />
                    <span asp-validation-for="QuantityInStock" class="text-danger"></span>
                </div>
            </div>

            <div class="mb-3">
                <label asp-for="CategoryId" class="form-label"></label>
                <select asp-for="CategoryId" class="form-select" asp-items="@Model.CategoryList">
                    <option value="">-- Select Category --</option>
                </select>
                <span asp-validation-for="CategoryId" class="text-danger"></span>
            </div>

            <div class="form-check form-switch mb-4">
                <input asp-for="IsActive" class="form-check-input" role="switch" />
                <label asp-for="IsActive" class="form-check-label"></label>
            </div>

            <div class="d-flex gap-2">
                <button type="submit" class="btn btn-primary">
                    <i class="fa-solid fa-floppy-disk me-1"></i> Save
                </button>
                <a asp-action="Index" class="btn btn-outline-secondary">
                    <i class="fa-solid fa-arrow-left me-1"></i> Cancel
                </a>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

**`Views/Products/Edit.cshtml`**
```cshtml
@model ProductViewModel

@{
    ViewData["Title"] = "Edit Product";
}

<h2 class="mb-4"><i class="fa-solid fa-pen me-2 text-warning"></i>Edit Product</h2>

<div class="card shadow-sm" style="max-width: 700px;">
    <div class="card-body">
        <form asp-action="Edit" method="post" novalidate>
            <input type="hidden" asp-for="Id" />
            <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

            <div class="mb-3">
                <label asp-for="Name" class="form-label"></label>
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="SKU" class="form-label"></label>
                <input asp-for="SKU" class="form-control" />
                <span asp-validation-for="SKU" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="Description" class="form-label"></label>
                <textarea asp-for="Description" class="form-control" rows="3"></textarea>
                <span asp-validation-for="Description" class="text-danger"></span>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="Price" class="form-label"></label>
                    <div class="input-group">
                        <span class="input-group-text">$</span>
                        <input asp-for="Price" class="form-control" type="number" step="0.01" min="0.01" />
                    </div>
                    <span asp-validation-for="Price" class="text-danger"></span>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="QuantityInStock" class="form-label"></label>
                    <input asp-for="QuantityInStock" class="form-control" type="number" min="0" />
                    <span asp-validation-for="QuantityInStock" class="text-danger"></span>
                </div>
            </div>

            <div class="mb-3">
                <label asp-for="CategoryId" class="form-label"></label>
                <select asp-for="CategoryId" class="form-select" asp-items="@Model.CategoryList">
                    <option value="">-- Select Category --</option>
                </select>
                <span asp-validation-for="CategoryId" class="text-danger"></span>
            </div>

            <div class="form-check form-switch mb-4">
                <input asp-for="IsActive" class="form-check-input" role="switch" />
                <label asp-for="IsActive" class="form-check-label"></label>
            </div>

            <div class="d-flex gap-2">
                <button type="submit" class="btn btn-warning text-white">
                    <i class="fa-solid fa-floppy-disk me-1"></i> Update
                </button>
                <a asp-action="Index" class="btn btn-outline-secondary">
                    <i class="fa-solid fa-arrow-left me-1"></i> Cancel
                </a>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

**`Views/Products/Delete.cshtml`**
```cshtml
@model Product

@{
    ViewData["Title"] = "Delete Product";
}

<h2 class="mb-4"><i class="fa-solid fa-trash me-2 text-danger"></i>Delete Product</h2>

<div class="alert alert-warning" style="max-width: 700px;">
    <i class="fa-solid fa-triangle-exclamation me-1"></i>
    Are you sure you want to delete this product? This action cannot be undone.
</div>

<div class="card shadow-sm" style="max-width: 700px;">
    <div class="card-body">
        <dl class="row mb-0">
            <dt class="col-sm-4">Product Name</dt>
            <dd class="col-sm-8">@Model.Name</dd>

            <dt class="col-sm-4">SKU</dt>
            <dd class="col-sm-8"><span class="badge bg-secondary">@Model.SKU</span></dd>

            <dt class="col-sm-4">Category</dt>
            <dd class="col-sm-8">@Model.Category?.Name</dd>

            <dt class="col-sm-4">Price</dt>
            <dd class="col-sm-8">@Model.Price.ToString("C")</dd>

            <dt class="col-sm-4">Quantity In Stock</dt>
            <dd class="col-sm-8">@Model.QuantityInStock</dd>
        </dl>
    </div>
    <div class="card-footer bg-white">
        <form asp-action="Delete" method="post">
            <input type="hidden" asp-for="Id" />
            <button type="submit" class="btn btn-danger">
                <i class="fa-solid fa-trash me-1"></i> Delete
            </button>
            <a asp-action="Index" class="btn btn-outline-secondary">
                <i class="fa-solid fa-arrow-left me-1"></i> Cancel
            </a>
        </form>
    </div>
</div>
```

**`wwwroot/css/site.css`**
```css
body {
  font-family: 'Segoe UI', Roboto, Arial, sans-serif;
}

.card {
  border: none;
  border-radius: 0.75rem;
}

.table thead th {
  font-weight: 600;
  font-size: 0.85rem;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.btn {
  border-radius: 0.5rem;
}

.navbar-brand i {
  color: #6ea8fe;
}
```

---

## 13. Functional Coverage Checklist

| Requirement | Covered |
|---|---|
| List products (Name, SKU, Category, Price, Qty, Active, Created, Actions) | ✅ `Index.cshtml` |
| Product details view | ✅ `Details.cshtml` |
| Create product form | ✅ `Create.cshtml` |
| Edit product form | ✅ `Edit.cshtml` |
| Delete with confirmation | ✅ `Delete.cshtml` |
| Async/await EF Core | ✅ All service methods |
| Validation & error handling | ✅ DataAnnotations + service-layer try/catch + ModelState |
| Interface + Service layer, controller calls service | ✅ `IProductService`/`ICategoryService` + `ProductsController` |
| Category dropdown, dynamic | ✅ `ICategoryService.GetActiveCategorySelectListAsync()` |
| Unique SKU / Category name | ✅ DB unique index + `SkuExistsAsync` check |
| Price > 0, Qty >= 0 | ✅ `Range` attributes + SQL `CHECK` constraints |
| Seed data (3+ categories, 8+ products) | ✅ `HasData` + `schema.sql` |
| Bootstrap UI | ✅ Bootstrap 5 via CDN throughout |

---

## 14. Assumptions

- Hard delete is used for products (soft delete was optional per the spec).
- No separate Category CRUD screens were built since categories are seeded via EF/SQL script; this can be added by creating an `ICategoryService.CreateAsync/UpdateAsync/DeleteAsync` plus a `CategoriesController`, mirroring the Products pattern.
- `CreatedUtc`/`UpdatedUtc` are stored in UTC with no timezone conversion layer, since none was specified.
- Bootstrap and Font Awesome are loaded via CDN for simplicity — bundle locally for offline/production use.
- Validation runs both client-side (unobtrusive jQuery validation) and server-side (`ModelState`).
- Services return a `(bool Success, string? Error)` tuple rather than throwing on expected failure paths (e.g. "not found"), keeping the controller's error handling simple and consistent.

## 15. Possible Next Steps

- Add pagination/search/sort to the Products index (in `IProductService`).
- Add a Categories management screen (Index/Create/Edit/Delete) with full `ICategoryService` CRUD.
- Add a unit test project that mocks `IProductService`/`ICategoryService` to test the controller in isolation.
- Add a repository layer beneath the services if you want to swap EF Core for another data source later.
