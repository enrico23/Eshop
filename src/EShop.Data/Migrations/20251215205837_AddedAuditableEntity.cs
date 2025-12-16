using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedAuditableEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedUtc",
                table: "Products",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedUtc",
                table: "Products",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedUtc",
                table: "Categories",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedUtc",
                table: "Categories",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("0b73b90f-9610-4f64-893e-37edc85c0a5f"),
                column: "UpdatedUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3a4b77df-1a28-4f7e-b764-c4d62a6f6ed8"),
                column: "UpdatedUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("49d2e88d-241d-4fdb-9d41-cf4a6c9ab3c9"),
                column: "UpdatedUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7fb5c7d1-c2a4-4ff2-9f76-4e0b55a389b6"),
                column: "UpdatedUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a3b4f1de-8241-4ff0-9c5e-af4c4c7a1e10"),
                column: "UpdatedUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c1e2d8ba-992f-4e8e-a0a9-5bcff3df8c77"),
                column: "UpdatedUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d0c52f04-4cd2-4e47-bc9f-06b251f8eae9"),
                column: "UpdatedUtc",
                value: null);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f5e1a4cc-2b9b-4db9-9b52-6d3f72440365"),
                column: "UpdatedUtc",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "Categories");
        }
    }
}
