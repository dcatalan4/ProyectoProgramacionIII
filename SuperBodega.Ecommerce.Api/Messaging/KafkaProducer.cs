using Confluent.Kafka;
using System.Text.Json;
using SuperBodega.Ecommerce.Api.Queues;

namespace SuperBodega.Ecommerce.Api.Messaging;

public class KafkaProducer : IPedidoQueue
{
    private readonly IProducer<Null, string> _producer;

    public KafkaProducer()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        _producer = new ProducerBuilder<Null, string>(config)
            .Build();
    }

    public async Task EnviarPedidoAsync(Guid carritoId)
    {
        var mensaje = new PedidoKafkaMessage
        {
            CarritoId = carritoId
        };

        var json = JsonSerializer.Serialize(mensaje);

        await _producer.ProduceAsync(
            "pedidos-topic",
            new Message<Null, string>
            {
                Value = json
            });

        Console.WriteLine($"Mensaje enviado a Kafka: {json}");
    }

    public ValueTask EnqueueAsync(
        PedidoQueueItem item,
        CancellationToken cancellationToken)
    {
        return new ValueTask(
            EnviarPedidoAsync(item.CarritoId));
    }

    public ValueTask<PedidoQueueItem> DequeueAsync(
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}