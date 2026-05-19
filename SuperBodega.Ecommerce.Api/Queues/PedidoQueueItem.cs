namespace SuperBodega.Ecommerce.Api.Queues;

public sealed record PedidoQueueItem(Guid SolicitudId, Guid CarritoId);
