using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class CarritoConfiguration : IEntityTypeConfiguration<Carrito>
{
    public void Configure(EntityTypeBuilder<Carrito> builder)
    {
        builder.ToTable("carritos");
        builder.HasKey(carrito => carrito.Id);
        builder.Ignore(carrito => carrito.Total);

        builder.HasOne(carrito => carrito.Cliente)
            .WithMany(cliente => cliente.Carritos)
            .HasForeignKey(carrito => carrito.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
