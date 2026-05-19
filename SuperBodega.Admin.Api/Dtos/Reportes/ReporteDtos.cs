using System.ComponentModel.DataAnnotations;

namespace SuperBodega.Admin.Api.Dtos.Reportes;

public sealed class ReportePeriodoRequest
{
    [Required]
    public DateTime Desde { get; set; }

    [Required]
    public DateTime Hasta { get; set; }
}

public sealed record ReportePeriodoResponse(
    DateTime Desde,
    DateTime Hasta,
    int TotalVentas,
    decimal MontoVentas,
    int TotalCompras,
    decimal MontoCompras);

public sealed record ReporteProductoResponse(
    Guid ProductoId,
    string? Producto,
    int UnidadesVendidas,
    decimal MontoVendido,
    int UnidadesCompradas,
    decimal MontoComprado);

public sealed record ReporteClienteResponse(
    Guid ClienteId,
    string? Cliente,
    int TotalVentas,
    decimal MontoComprado);

public sealed record ReporteProveedorResponse(
    Guid ProveedorId,
    string? Proveedor,
    int TotalCompras,
    decimal MontoComprado,
    int ProductosActivos);
