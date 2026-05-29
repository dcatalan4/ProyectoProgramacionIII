using Microsoft.EntityFrameworkCore;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data;

public sealed class SuperBodegaDbContext(DbContextOptions<SuperBodegaDbContext> options) : DbContext(options)
{
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetallesCompra => Set<DetalleCompra>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Carrito> Carritos => Set<Carrito>();
    public DbSet<CarritoDetalle> CarritoDetalles => Set<CarritoDetalle>();
    public DbSet<NotificacionPedido> NotificacionesPedido => Set<NotificacionPedido>();
    public DbSet<SolicitudPedido> SolicitudPedidos => Set<SolicitudPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SuperBodegaDbContext).Assembly);
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var frutasId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var lacteosId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var proveedorId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = frutasId, Nombre = "Frutas", Descripcion = "Frutas frescas de temporada" },
            new Categoria { Id = lacteosId, Nombre = "Lacteos", Descripcion = "Leche, quesos y derivados" });

        modelBuilder.Entity<Proveedor>().HasData(
            new Proveedor
            {
                Id = proveedorId,
                Nombre = "Distribuidora Central",
                Nit = "CF",
                Telefono = "5555-0101",
                Email = "ventas@distribuidoracentral.test"
            });

        modelBuilder.Entity<Producto>().HasData(
            new Producto
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Sku = "FRU-MAN-001",
                Nombre = "Manzana roja",
                Descripcion = "Manzana fresca por unidad",
                PrecioCompra = 0.80m,
                PrecioVenta = 1.25m,
                Stock = 180,
                CategoriaId = frutasId,
                ProveedorId = proveedorId
            },
            new Producto
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Sku = "LAC-LEC-001",
                Nombre = "Leche entera 1L",
                Descripcion = "Leche entera pasteurizada",
                PrecioCompra = 0.75m,
                PrecioVenta = 1.15m,
                Stock = 95,
                CategoriaId = lacteosId,
                ProveedorId = proveedorId
            });
    }
}
