using System.Threading.Channels;

namespace SuperBodega.Ecommerce.Api.Queues;

public sealed class InMemoryPedidoQueue : IPedidoQueue
{
    private readonly Channel<PedidoQueueItem> _channel = Channel.CreateUnbounded<PedidoQueueItem>();

    public ValueTask EnqueueAsync(PedidoQueueItem item, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(item, cancellationToken);
    }

    public ValueTask<PedidoQueueItem> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
