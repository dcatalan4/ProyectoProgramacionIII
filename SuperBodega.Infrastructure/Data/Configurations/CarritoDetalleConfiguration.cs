using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class CarritoDetalleConfiguration : IEntityTypeConfiguration<CarritoDetalle>
{
    public void Configure(EntityTypeBuilder<CarritoDetalle> builder)
    {
        builder.ToTable("carrito_detalles");
        builder.HasKey(detalle => detalle.Id);
        builder.Property(detalle => detalle.PrecioUnitario).HasPrecision(12, 2);
        builder.Ignore(detalle => detalle.Subtotal);
        builder.HasIndex(detalle => new { detalle.CarritoId, detalle.ProductoId }).IsUnique();

        builder.HasOne(detalle => detalle.Carrito)
            .WithMany(carrito => carrito.Detalles)
            .HasForeignKey(detalle => detalle.CarritoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(detalle => detalle.Producto)
            .WithMany(producto => producto.CarritoDetalles)
            .HasForeignKey(detalle => detalle.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
