using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBodega.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdOriginalToProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdOriginal",
                table: "productos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "IdOriginal",
                value: "");

            migrationBuilder.UpdateData(
                table: "productos",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "IdOriginal",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdOriginal",
                table: "productos");
        }
    }
}
