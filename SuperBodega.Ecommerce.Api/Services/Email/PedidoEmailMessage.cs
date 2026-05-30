using SuperBodega.Domain.Entities;

namespace SuperBodega.Ecommerce.Api.Services.Email;

public sealed record PedidoEmailMessage(
    string NumeroVenta,
    DateTime FechaUtc,
    Cliente Cliente,
    decimal Total,
    IReadOnlyCollection<PedidoEmailDetalle> Detalles);

public sealed record PedidoEmailDetalle(
    string Producto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal);
