using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBodega.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFKVentasClientesToAllowNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ventas_clientes_ClienteId",
                table: "ventas");

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_clientes_ClienteId",
                table: "ventas",
                column: "ClienteId",
                principalTable: "clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ventas_clientes_ClienteId",
                table: "ventas");

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_clientes_ClienteId",
                table: "ventas",
                column: "ClienteId",
                principalTable: "clientes",
                principalColumn: "Id");
        }
    }
}
