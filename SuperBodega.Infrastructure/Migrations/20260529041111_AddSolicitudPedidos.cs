using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperBodega.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitudPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudPedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CarritoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    CreadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcesadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MensajeError = table.Column<string>(type: "text", nullable: true),
                    VentaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudPedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudPedidos_ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "ventas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudPedidos_VentaId",
                table: "SolicitudPedidos",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudPedidos");
        }
    }
}
