using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBodega.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdOriginalToProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdOriginal",
                table: "proveedores",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "proveedores",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "IdOriginal",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdOriginal",
                table: "proveedores");
        }
    }
}
