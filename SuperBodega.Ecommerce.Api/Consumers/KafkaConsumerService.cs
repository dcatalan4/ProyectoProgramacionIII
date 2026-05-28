using Confluent.Kafka;
using System.Text.Json;
using SuperBodega.Ecommerce.Api.Messaging;
using SuperBodega.Ecommerce.Api.Services.Pedidos;

namespace SuperBodega.Ecommerce.Api.Consumers;

public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<Ignore, string> _consumer;

    public KafkaConsumerService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "pedidos-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config)
            .Build();

        _consumer.Subscribe("pedidos-topic");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Kafka Consumer iniciado.");

        return Task.Run(() => StartConsuming(stoppingToken), stoppingToken);
    }

    private async Task StartConsuming(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(
                    TimeSpan.FromSeconds(1));

                if (result is null)
                {
                    continue;
                }

                Console.WriteLine($"Mensaje recibido: {result.Message.Value}");

                var pedido = JsonSerializer.Deserialize<PedidoKafkaMessage>(
                    result.Message.Value);

                if (pedido is null)
                {
                    continue;
                }

                using var scope = _serviceProvider.CreateScope();

                var pedidoService = scope.ServiceProvider
                    .GetRequiredService<IPedidoService>();

                var response = await pedidoService.ProcesarCarritoAsync(
                    pedido.CarritoId,
                    stoppingToken);

                if (response.Success)
                {
                    Console.WriteLine("Pedido procesado correctamente.");
                }
                else
                {
                    Console.WriteLine($"Error procesando pedido: {response.Error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Kafka error: {ex.Message}");
        }
        finally
        {
            _consumer.Close();
        }
    }
}
