using SuperBodega.Ecommerce.Api.Queues;
using SuperBodega.Ecommerce.Api.Services.Pedidos;

namespace SuperBodega.Ecommerce.Api.Workers;

public sealed class PedidoQueueWorker(
    IPedidoQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<PedidoQueueWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var item = await queue.DequeueAsync(stoppingToken);

            using var scope = scopeFactory.CreateScope();
            var pedidos = scope.ServiceProvider.GetRequiredService<IPedidoService>();
            var result = await pedidos.ProcesarCarritoAsync(item.CarritoId, stoppingToken);

            if (result.Success)
            {
                logger.LogInformation("Pedido asincrono {solicitudId} procesado para carrito {carritoId}.", item.SolicitudId, item.CarritoId);
            }
            else
            {
                logger.LogWarning("Pedido asincrono {solicitudId} no pudo procesarse: {error}", item.SolicitudId, result.Error);
            }
        }
    }
}
