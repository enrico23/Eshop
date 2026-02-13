using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogImageConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_CatalogImages_CategoryId",
                table: "CatalogImages");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_IsPrimary",
                table: "ProductImages",
                columns: new[] { "ProductId", "IsPrimary" },
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_Url",
                table: "ProductImages",
                columns: new[] { "ProductId", "Url" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductImages_SortOrder_NonNegative",
                table: "ProductImages",
                sql: "[SortOrder] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogImages_CategoryId_IsPrimary",
                table: "CatalogImages",
                columns: new[] { "CategoryId", "IsPrimary" },
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogImages_CategoryId_Url",
                table: "CatalogImages",
                columns: new[] { "CategoryId", "Url" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CatalogImages_SortOrder_NonNegative",
                table: "CatalogImages",
                sql: "[SortOrder] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_IsPrimary",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_Url",
                table: "ProductImages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductImages_SortOrder_NonNegative",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_CatalogImages_CategoryId_IsPrimary",
                table: "CatalogImages");

            migrationBuilder.DropIndex(
                name: "IX_CatalogImages_CategoryId_Url",
                table: "CatalogImages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CatalogImages_SortOrder_NonNegative",
                table: "CatalogImages");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogImages_CategoryId",
                table: "CatalogImages",
                column: "CategoryId");
        }
    }
}
