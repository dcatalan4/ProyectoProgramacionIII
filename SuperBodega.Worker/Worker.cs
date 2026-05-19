using Microsoft.EntityFrameworkCore;
using SuperBodega.Infrastructure.Data;

namespace SuperBodega.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SuperBodegaDbContext>();
                var productosActivos = await dbContext.Productos
                    .CountAsync(producto => producto.EstaActivo, stoppingToken);

                _logger.LogInformation(
                    "Inventario activo de SuperBodega: {count} productos a las {time}",
                    productosActivos,
                    DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
