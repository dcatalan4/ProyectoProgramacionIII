namespace SuperBodega.Ecommerce.Api.Queues;

public interface IPedidoQueue
{
    ValueTask EnqueueAsync(PedidoQueueItem item, CancellationToken cancellationToken);
    ValueTask<PedidoQueueItem> DequeueAsync(CancellationToken cancellationToken);
}
