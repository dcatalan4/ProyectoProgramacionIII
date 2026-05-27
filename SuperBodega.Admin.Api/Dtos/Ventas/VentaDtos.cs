using System.ComponentModel.DataAnnotations;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Admin.Api.Dtos.Ventas;

public sealed record VentaResponse(
    Guid Id,
    string IdOriginal,
    string NumeroVenta,
    DateTime FechaUtc,
    EstadoVenta Estado,
    string ClienteId,
    string? Cliente,
    decimal Total,
    IReadOnlyCollection<DetalleVentaResponse> Detalles);

public sealed record DetalleVentaResponse(
    Guid Id,
    string ProductoId,
    string? Producto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);

public class CrearVentaRequest
{
    [Required, MinLength(4)]
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string NumeroVenta { get; set; } = string.Empty;

    [Required, MinLength(4)]
    public string ClienteId { get; set; } = string.Empty;

    [Required]
    public DateTime Fecha { get; set; }

    [MinLength(1)]
    public List<CrearDetalleVentaRequest> Detalles { get; set; } = [];
}

public sealed class CrearDetalleVentaRequest
{
    [Required, MinLength(4)]
    public string ProductoId { get; set; } = string.Empty;

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
