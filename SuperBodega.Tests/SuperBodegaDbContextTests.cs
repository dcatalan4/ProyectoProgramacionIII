using Microsoft.EntityFrameworkCore;
using SuperBodega.Domain.Entities;
using SuperBodega.Infrastructure.Data;

namespace SuperBodega.Tests;

public sealed class SuperBodegaDbContextTests
{
    [Fact]
    public void Model_IncludesOnlineSupermarketEntities()
    {
        using var dbContext = CreateDbContext();

        var entityNames = dbContext.Model.GetEntityTypes()
            .Select(entity => entity.ClrType.Name)
            .ToArray();

        Assert.Contains(nameof(Producto), entityNames);
        Assert.Contains(nameof(Categoria), entityNames);
        Assert.Contains(nameof(Proveedor), entityNames);
        Assert.Contains(nameof(Cliente), entityNames);
        Assert.Contains(nameof(Compra), entityNames);
        Assert.Contains(nameof(DetalleCompra), entityNames);
        Assert.Contains(nameof(Venta), entityNames);
        Assert.Contains(nameof(DetalleVenta), entityNames);
        Assert.Contains(nameof(Carrito), entityNames);
        Assert.Contains(nameof(CarritoDetalle), entityNames);
        Assert.Contains(nameof(NotificacionPedido), entityNames);
    }

    [Fact]
    public void Producto_HasCategoriaAndProveedorRelationships()
    {
        using var dbContext = CreateDbContext();

        var producto = dbContext.Model.FindEntityType(typeof(Producto));

        Assert.NotNull(producto?.FindNavigation(nameof(Producto.Categoria)));
        Assert.NotNull(producto?.FindNavigation(nameof(Producto.Proveedor)));
    }

    private static SuperBodegaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SuperBodegaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SuperBodegaDbContext(options);
    }
}
