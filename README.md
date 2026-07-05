# Product Management System (.NET 10.0 ASP.NET Core MVC)

This project is an **ASP.NET Core 10.0 MVC** web application designed to manage a product catalog. It features full CRUD operations for Products, Category selection, input validation, and database seeding utilizing **Entity Framework Core 10 (EF Core)** and **SQL Server**.

The project is structured following clean architecture principles, leveraging dependency injection, the service layer pattern to keep controllers lean, and strongly typed Razor views styled with Bootstrap.

---

## 📂 Repository Directory Structure

```text
ProductManagement/
├── ProductManagement/
│   ├── Controllers/             # MVC Controllers (handling UI flows)
│   ├── Data/                    # Database Context & Seed Configuration
│   ├── Migrations/              # EF Core database migrations
│   ├── Models/                  # Domain entities & ViewModels
│   ├── Properties/              # Launch settings
│   ├── Services/                # Business logic services & interfaces
│   ├── Views/                   # Razor views (.cshtml) for UI presentation
│   ├── wwwroot/                 # Static files (CSS, JS, Bootstrap)
│   ├── Program.cs               # App initialization, services registration, & pipeline
│   ├── appsettings.json         # Settings & SQL connection string configurations
│   └── ProductManagement.csproj # Project dependency file (.NET 10.0)
```

---

## 📝 Folder & Layer Descriptions

### 🎛️ [Controllers](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Controllers)
Contains the [ProductsController.cs](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Controllers/ProductsController.cs) which coordinates application execution. It interacts with the service layers rather than accessing the database directly, keeping the HTTP controller slim and focused purely on request/response handling.

### 📦 [Models](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Models)
Defines the data structures of the application:
*   [Product.cs](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Models/Product.cs): Represents the database-mapped Product entity with properties like SKU, Name, Price, and Stock, along with validation annotations.
*   [Category.cs](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Models/Category.cs): Represents the Category entity, configured with a one-to-many relationship with Products.
*   [ProductViewModel.cs](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Models/ProductViewModel.cs): Used to transport data safely between views and controllers, including category drop-down list selections.

### ⚙️ [Services](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Services)
Encapsulates all business logic separated from the database access layer:
*   **Interfaces** ([ICategoryService.cs](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Services/Interfaces/ICategoryService.cs), [IProductService.cs](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Services/Interfaces/IProductService.cs)): Expose business method contracts to maximize code testability.
*   **Implementations** ([CategoryService.cs](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Services/CategoryService.cs), [ProductService.cs](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Services/ProductService.cs)): Contain transactional workflows, error handling, SKU collision validations, and asynchronous EF Core method execution.

### 💾 [Data](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Data)
Hosts the EF Core database infrastructure.
*   [ApplicationDbContext.cs](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Data/ApplicationDbContext.cs): Exposes database sets (`DbSet`) for Products and Categories. It specifies relational rules (like unique constraints on SKU/Category Name, and restrict-on-delete policies) and seeds initial test data (3 categories, 8 products).

### 🎨 [Views](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Views)
Contains Razor view templates (.cshtml files) rendering the HTML interface:
*   **Products Views** ([Products Views Directory](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Views/Products)): Includes templates for CRUD operations (`Index`, `Create`, `Edit`, `Details`, `Delete`).
*   **Shared Layouts** ([Shared Directory](file:///D:/repos/ProductManagement/ProductManagement/ProductManagement/Views/Shared)): Includes `_Layout.cshtml` configuring the global HTML structure, navbar, and script includes.
