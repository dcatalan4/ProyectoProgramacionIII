using System.ComponentModel.DataAnnotations;

namespace SuperBodega.Admin.Api.Dtos.Compras;

public sealed record CompraResponse(
    Guid Id,
    string NumeroCompra,
    DateTime FechaUtc,
    Guid ProveedorId,
    string? Proveedor,
    decimal Total,
    IReadOnlyCollection<DetalleCompraResponse> Detalles);

public sealed record DetalleCompraResponse(
    Guid Id,
    Guid ProductoId,
    string? Producto,
    int Cantidad,
    decimal CostoUnitario,
    decimal Subtotal);

public class CrearCompraRequest
{
    [Required, MaxLength(40)]
    public string NumeroCompra { get; set; } = string.Empty;

    [Required]
    public Guid ProveedorId { get; set; }

    [MinLength(1)]
    public List<CrearDetalleCompraRequest> Detalles { get; set; } = [];
}

public sealed class CrearDetalleCompraRequest
{
    [Required]
    public Guid ProductoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal CostoUnitario { get; set; }
}

public sealed class ActualizarCompraRequest : CrearCompraRequest;
