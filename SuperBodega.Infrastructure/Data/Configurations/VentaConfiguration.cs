using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("ventas");
        builder.HasKey(venta => venta.Id);
        builder.Property(venta => venta.NumeroVenta).HasMaxLength(40).IsRequired();
        builder.HasIndex(venta => venta.NumeroVenta).IsUnique();
        builder.Ignore(venta => venta.Total);

        builder.HasOne(venta => venta.Cliente)
            .WithMany(cliente => cliente.Ventas)
            .HasForeignKey(venta => venta.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
