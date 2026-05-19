using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("productos");
        builder.HasKey(producto => producto.Id);
        builder.Property(producto => producto.Sku).HasMaxLength(40).IsRequired();
        builder.Property(producto => producto.Nombre).HasMaxLength(160).IsRequired();
        builder.Property(producto => producto.Descripcion).HasMaxLength(500);
        builder.Property(producto => producto.PrecioVenta).HasPrecision(12, 2);
        builder.Property(producto => producto.PrecioCompra).HasPrecision(12, 2);
        builder.HasIndex(producto => producto.Sku).IsUnique();

        builder.HasOne(producto => producto.Categoria)
            .WithMany(categoria => categoria.Productos)
            .HasForeignKey(producto => producto.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(producto => producto.Proveedor)
            .WithMany(proveedor => proveedor.Productos)
            .HasForeignKey(producto => producto.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
