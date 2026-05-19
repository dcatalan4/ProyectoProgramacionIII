using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> builder)
    {
        builder.ToTable("compras");
        builder.HasKey(compra => compra.Id);
        builder.Property(compra => compra.NumeroCompra).HasMaxLength(40).IsRequired();
        builder.HasIndex(compra => compra.NumeroCompra).IsUnique();
        builder.Ignore(compra => compra.Total);

        builder.HasOne(compra => compra.Proveedor)
            .WithMany(proveedor => proveedor.Compras)
            .HasForeignKey(compra => compra.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
