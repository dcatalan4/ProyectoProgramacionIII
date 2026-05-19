using System.ComponentModel.DataAnnotations;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Ecommerce.Api.Dtos.Carrito;

public sealed class CrearCarritoRequest
{
    [Required]
    public Guid ClienteId { get; set; }
}

public sealed class AgregarCarritoItemRequest
{
    [Required]
    public Guid CarritoId { get; set; }

    [Required]
    public Guid ProductoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}

public sealed record CarritoResponse(
    Guid Id,
    Guid ClienteId,
    EstadoCarrito Estado,
    DateTime CreadoUtc,
    DateTime? ActualizadoUtc,
    decimal Total,
    IReadOnlyCollection<CarritoItemResponse> Items);

public sealed record CarritoItemResponse(
    Guid Id,
    Guid ProductoId,
    string? Producto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);
