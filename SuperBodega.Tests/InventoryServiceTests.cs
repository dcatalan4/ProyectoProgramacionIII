using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SuperBodega.Admin.Api.Dtos.Compras;
using SuperBodega.Admin.Api.Dtos.Ventas;
using SuperBodega.Admin.Api.Services.Compras;
using SuperBodega.Admin.Api.Services.Ventas;
using SuperBodega.Domain.Entities;
using SuperBodega.Infrastructure.Data;

namespace SuperBodega.Tests;

public sealed class InventoryServiceTests
{
    [Fact]
    public async Task CreateCompra_IncreasesProductStock()
    {
        using var dbContext = CreateDbContext();
        var seed = await SeedAsync(dbContext, stock: 5);
        var service = new CompraService(dbContext);

        var result = await service.CreateAsync(new CrearCompraRequest
        {
            NumeroCompra = "C-001",
            ProveedorId = seed.ProveedorId,
            Detalles =
            [
                new CrearDetalleCompraRequest
                {
                    ProductoId = seed.ProductoId,
                    Cantidad = 7,
                    CostoUnitario = 2.50m
                }
            ]
        }, CancellationToken.None);

        var producto = await dbContext.Productos.FindAsync(seed.ProductoId);
        Assert.True(result.Success);
        Assert.Equal(12, producto?.Stock);
    }

    [Fact]
    public async Task CreateVenta_DecreasesProductStock()
    {
        using var dbContext = CreateDbContext();
        var seed = await SeedAsync(dbContext, stock: 10);
        var service = new VentaService(dbContext);

        var result = await service.CreateAsync(new CrearVentaRequest
        {
            NumeroVenta = "V-001",
            ClienteId = seed.ClienteId,
            Detalles =
            [
                new CrearDetalleVentaRequest
                {
                    ProductoId = seed.ProductoId,
                    Cantidad = 4
                }
            ]
        }, CancellationToken.None);

        var producto = await dbContext.Productos.FindAsync(seed.ProductoId);
        Assert.True(result.Success);
        Assert.Equal(6, producto?.Stock);
    }

    [Fact]
    public async Task CreateVenta_ReturnsErrorWhenStockIsInsufficient()
    {
        using var dbContext = CreateDbContext();
        var seed = await SeedAsync(dbContext, stock: 3);
        var service = new VentaService(dbContext);

        var result = await service.CreateAsync(new CrearVentaRequest
        {
            NumeroVenta = "V-002",
            ClienteId = seed.ClienteId,
            Detalles =
            [
                new CrearDetalleVentaRequest
                {
                    ProductoId = seed.ProductoId,
                    Cantidad = 4
                }
            ]
        }, CancellationToken.None);

        var producto = await dbContext.Productos.FindAsync(seed.ProductoId);
        Assert.False(result.Success);
        Assert.Contains("Stock insuficiente", result.Error);
        Assert.Equal(3, producto?.Stock);
    }

    private static SuperBodegaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SuperBodegaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new SuperBodegaDbContext(options);
    }

    private static async Task<(Guid ProductoId, Guid ProveedorId, Guid ClienteId)> SeedAsync(
        SuperBodegaDbContext dbContext,
        int stock)
    {
        var categoria = new Categoria { Nombre = "Abarrotes" };
        var proveedor = new Proveedor { Nombre = "Proveedor Test" };
        var cliente = new Cliente
        {
            Nombre = "Cliente",
            Apellido = "Test",
            Email = $"{Guid.NewGuid()}@test.local"
        };
        var producto = new Producto
        {
            Sku = Guid.NewGuid().ToString("N"),
            Nombre = "Producto Test",
            PrecioCompra = 1.00m,
            PrecioVenta = 2.00m,
            Stock = stock,
            Categoria = categoria,
            Proveedor = proveedor
        };

        dbContext.AddRange(categoria, proveedor, cliente, producto);
        await dbContext.SaveChangesAsync();

        return (producto.Id, proveedor.Id, cliente.Id);
    }
}
