using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        builder.ToTable("detalles_venta");
        builder.HasKey(detalle => detalle.Id);
        builder.Property(detalle => detalle.PrecioUnitario).HasPrecision(12, 2);
        builder.Ignore(detalle => detalle.Subtotal);

        builder.HasOne(detalle => detalle.Venta)
            .WithMany(venta => venta.Detalles)
            .HasForeignKey(detalle => detalle.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(detalle => detalle.Producto)
            .WithMany(producto => producto.DetallesVenta)
            .HasForeignKey(detalle => detalle.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
