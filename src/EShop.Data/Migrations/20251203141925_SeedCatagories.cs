using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatagories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("222da78d-127c-421b-8557-71822a136919"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3e106e1e-61e6-41d8-9501-6fa9e45788eb"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("406ba288-34f2-43a1-81b8-70cc86c24a08"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("89b91d0e-194e-4d98-9369-43aafc81e6c1"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("8caf2d61-ac1d-4a3d-a079-3fd3e9c14467"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("af0a1165-8d4c-427b-a398-23faf7db1acb"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c44d44dc-daa9-414c-a0bb-c4a985c30c3f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ddee007f-1026-4272-92de-5c2f19c8298a"));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("0b73b90f-9610-4f64-893e-37edc85c0a5f"), "Tablets" },
                    { new Guid("3a4b77df-1a28-4f7e-b764-c4d62a6f6ed8"), "Cameras" },
                    { new Guid("49d2e88d-241d-4fdb-9d41-cf4a6c9ab3c9"), "Wearables" },
                    { new Guid("7fb5c7d1-c2a4-4ff2-9f76-4e0b55a389b6"), "Audio" },
                    { new Guid("a3b4f1de-8241-4ff0-9c5e-af4c4c7a1e10"), "Laptops" },
                    { new Guid("c1e2d8ba-992f-4e8e-a0a9-5bcff3df8c77"), "Desktops" },
                    { new Guid("d0c52f04-4cd2-4e47-bc9f-06b251f8eae9"), "Gaming" },
                    { new Guid("f5e1a4cc-2b9b-4db9-9b52-6d3f72440365"), "Smartphones" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("0b73b90f-9610-4f64-893e-37edc85c0a5f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3a4b77df-1a28-4f7e-b764-c4d62a6f6ed8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("49d2e88d-241d-4fdb-9d41-cf4a6c9ab3c9"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7fb5c7d1-c2a4-4ff2-9f76-4e0b55a389b6"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a3b4f1de-8241-4ff0-9c5e-af4c4c7a1e10"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c1e2d8ba-992f-4e8e-a0a9-5bcff3df8c77"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d0c52f04-4cd2-4e47-bc9f-06b251f8eae9"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f5e1a4cc-2b9b-4db9-9b52-6d3f72440365"));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("222da78d-127c-421b-8557-71822a136919"), "Desktops" },
                    { new Guid("3e106e1e-61e6-41d8-9501-6fa9e45788eb"), "Audio" },
                    { new Guid("406ba288-34f2-43a1-81b8-70cc86c24a08"), "Wearables" },
                    { new Guid("89b91d0e-194e-4d98-9369-43aafc81e6c1"), "Smartphones" },
                    { new Guid("8caf2d61-ac1d-4a3d-a079-3fd3e9c14467"), "Gaming" },
                    { new Guid("af0a1165-8d4c-427b-a398-23faf7db1acb"), "Cameras" },
                    { new Guid("c44d44dc-daa9-414c-a0bb-c4a985c30c3f"), "Laptops" },
                    { new Guid("ddee007f-1026-4272-92de-5c2f19c8298a"), "Tablets" }
                });
        }
    }
}
