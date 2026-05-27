using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBodega.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdOriginalToCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdOriginal",
                table: "clientes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdOriginal",
                table: "categorias",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "IdOriginal",
                value: "");

            migrationBuilder.UpdateData(
                table: "categorias",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "IdOriginal",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdOriginal",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "IdOriginal",
                table: "categorias");
        }
    }
}
