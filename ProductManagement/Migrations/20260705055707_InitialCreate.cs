using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityInStock = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedUtc", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Electronic devices and accessories", true, "Electronics" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Office and stationery items", true, "Office Supplies" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Home and office furniture", true, "Furniture" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedUtc", "Description", "IsActive", "Name", "Price", "QuantityInStock", "SKU", "UpdatedUtc" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ergonomic wireless mouse", true, "Wireless Mouse", 19.99m, 150, "ELEC-001", null },
                    { 2, 1, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "RGB backlit mechanical keyboard", true, "Mechanical Keyboard", 59.99m, 80, "ELEC-002", null },
                    { 3, 1, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Full HD IPS monitor", true, "27-inch Monitor", 179.99m, 40, "ELEC-003", null },
                    { 4, 2, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "500 sheets, 80gsm", true, "A4 Paper Ream", 4.99m, 500, "OFF-001", null },
                    { 5, 2, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pack of 10 blue pens", true, "Ballpoint Pen Pack", 3.49m, 300, "OFF-002", null },
                    { 6, 2, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heavy duty stapler", true, "Stapler", 8.99m, 120, "OFF-003", null },
                    { 7, 3, new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ergonomic mesh office chair", true, "Office Chair", 149.99m, 25, "FURN-001", null },
                    { 8, 3, new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Height-adjustable standing desk", true, "Standing Desk", 299.99m, 15, "FURN-002", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
