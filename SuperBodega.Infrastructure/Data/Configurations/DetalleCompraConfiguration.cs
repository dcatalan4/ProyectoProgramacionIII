using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class DetalleCompraConfiguration : IEntityTypeConfiguration<DetalleCompra>
{
    public void Configure(EntityTypeBuilder<DetalleCompra> builder)
    {
        builder.ToTable("detalles_compra");
        builder.HasKey(detalle => detalle.Id);
        builder.Property(detalle => detalle.CostoUnitario).HasPrecision(12, 2);
        builder.Ignore(detalle => detalle.Subtotal);

        builder.HasOne(detalle => detalle.Compra)
            .WithMany(compra => compra.Detalles)
            .HasForeignKey(detalle => detalle.CompraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(detalle => detalle.Producto)
            .WithMany(producto => producto.DetallesCompra)
            .HasForeignKey(detalle => detalle.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
