using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SuperBodega.Domain.Entities;
using SuperBodega.Ecommerce.Api.Dtos.Pedidos;
using SuperBodega.Ecommerce.Api.Messaging;
using SuperBodega.Ecommerce.Api.Queues;
using SuperBodega.Ecommerce.Api.Services.Pedidos;
using SuperBodega.Infrastructure.Data;

namespace SuperBodega.Tests;

public sealed class PedidoSincronoServiceTests
{
    [Fact]
    public async Task CrearSincrono_ProcessesSaleInventoryAndNotificationDirectly()
    {
        using var dbContext = CreateDbContext();
        var seed = await SeedCartAsync(dbContext, stock: 10, quantity: 3);
        var service = new PedidoService(dbContext, new KafkaProducer());

        var result = await service.CrearSincronoAsync(new CrearPedidoRequest
        {
            CarritoId = seed.CarritoId
        }, CancellationToken.None);

        var producto = await dbContext.Productos.FindAsync(seed.ProductoId);
        var carrito = await dbContext.Carritos.FindAsync(seed.CarritoId);
        var venta = await dbContext.Ventas
            .Include(item => item.Detalles)
            .Include(item => item.NotificacionesPedido)
            .SingleAsync();

        Assert.True(result.Success);
        Assert.Equal(7, producto?.Stock);
        Assert.Equal(EstadoCarrito.Cerrado, carrito?.Estado);
        Assert.Single(venta.Detalles);
        Assert.Single(venta.NotificacionesPedido);
        Assert.True(venta.NotificacionesPedido.Single().FueEnviada);
        Assert.Equal(seed.ClienteEmail, result.Value?.Notificacion.Destinatario);
    }

    private static SuperBodegaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SuperBodegaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new SuperBodegaDbContext(options);
    }

    private static async Task<(Guid CarritoId, Guid ProductoId, string ClienteEmail)> SeedCartAsync(
        SuperBodegaDbContext dbContext,
        int stock,
        int quantity)
    {
        var cliente = new Cliente
        {
            Nombre = "Cliente",
            Apellido = "Pedido",
            Email = $"{Guid.NewGuid()}@test.local"
        };
        var categoria = new Categoria { Nombre = "Despensa" };
        var proveedor = new Proveedor { Nombre = "Proveedor Pedido" };
        var producto = new Producto
        {
            Sku = Guid.NewGuid().ToString("N"),
            Nombre = "Producto Pedido",
            PrecioCompra = 1.00m,
            PrecioVenta = 3.00m,
            Stock = stock,
            Categoria = categoria,
            Proveedor = proveedor
        };
        var carrito = new Carrito
        {
            Cliente = cliente,
            Detalles =
            [
                new CarritoDetalle
                {
                    Producto = producto,
                    Cantidad = quantity,
                    PrecioUnitario = producto.PrecioVenta
                }
            ]
        };

        dbContext.Add(carrito);
        await dbContext.SaveChangesAsync();

        return (carrito.Id, producto.Id, cliente.Email);
    }
}