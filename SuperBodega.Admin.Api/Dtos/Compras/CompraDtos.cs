using System.ComponentModel.DataAnnotations;

namespace SuperBodega.Admin.Api.Dtos.Compras;

public sealed record CompraResponse(
    Guid Id,
    string IdOriginal,
    string NumeroCompra,
    DateTime FechaUtc,
    string ProveedorId,
    string? Proveedor,
    decimal Total,
    IReadOnlyCollection<DetalleCompraResponse> Detalles);

public sealed record DetalleCompraResponse(
    Guid Id,
    string ProductoId,
    string? Producto,
    int Cantidad,
    decimal CostoUnitario,
    decimal Subtotal);

public class CrearCompraRequest
{
    [Required, MinLength(4)]
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string NumeroCompra { get; set; } = string.Empty;

    [Required, MinLength(4)]
    public string ProveedorId { get; set; } = string.Empty;

    [Required]
    public DateTime Fecha { get; set; }

    [MinLength(1)]
    public List<CrearDetalleCompraRequest> Detalles { get; set; } = [];
}

public sealed class CrearDetalleCompraRequest
{
    [Required, MinLength(4)]
    public string ProductoId { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal CostoUnitario { get; set; }
}

public sealed class ActualizarCompraRequest : CrearCompraRequest;
