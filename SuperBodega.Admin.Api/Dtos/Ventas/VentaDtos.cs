using System.ComponentModel.DataAnnotations;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Admin.Api.Dtos.Ventas;

public sealed record VentaResponse(
    Guid Id,
    string NumeroVenta,
    DateTime FechaUtc,
    EstadoVenta Estado,
    Guid ClienteId,
    string? Cliente,
    decimal Total,
    IReadOnlyCollection<DetalleVentaResponse> Detalles);

public sealed record DetalleVentaResponse(
    Guid Id,
    Guid ProductoId,
    string? Producto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);

public class CrearVentaRequest
{
    [Required, MaxLength(40)]
    public string NumeroVenta { get; set; } = string.Empty;

    [Required]
    public Guid ClienteId { get; set; }

    [MinLength(1)]
    public List<CrearDetalleVentaRequest> Detalles { get; set; } = [];
}

public sealed class CrearDetalleVentaRequest
{
    [Required]
    public Guid ProductoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}

public sealed class ActualizarVentaRequest : CrearVentaRequest
{
    public EstadoVenta Estado { get; set; } = EstadoVenta.Recibida;
}

public sealed class CambiarEstadoVentaRequest
{
    [Required]
    public EstadoVenta Estado { get; set; }
}
