using System.ComponentModel.DataAnnotations;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Ecommerce.Api.Dtos.Pedidos;

public sealed class CrearPedidoRequest
{
    [Required]
    public Guid CarritoId { get; set; }
}

public sealed record PedidoResponse(
    Guid VentaId,
    string NumeroVenta,
    EstadoVenta Estado,
    Guid ClienteId,
    decimal Total,
    PedidoNotificacionResponse Notificacion,
    IReadOnlyCollection<PedidoDetalleResponse> Detalles);

public sealed record PedidoDetalleResponse(
    Guid ProductoId,
    string? Producto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);

public sealed record PedidoNotificacionResponse(
    Guid Id,
    string Destinatario,
    string Mensaje,
    bool FueEnviada,
    DateTime? EnviadaUtc);

public sealed record PedidoEncoladoResponse(
    Guid SolicitudId,
    Guid CarritoId,
    string Estado);
