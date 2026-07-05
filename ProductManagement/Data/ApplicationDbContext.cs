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