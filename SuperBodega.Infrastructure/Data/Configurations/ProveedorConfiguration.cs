using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("proveedores");
        builder.HasKey(proveedor => proveedor.Id);
        builder.Property(proveedor => proveedor.Nombre).HasMaxLength(160).IsRequired();
        builder.Property(proveedor => proveedor.Nit).HasMaxLength(30);
        builder.Property(proveedor => proveedor.Telefono).HasMaxLength(30);
        builder.Property(proveedor => proveedor.Email).HasMaxLength(160);
        builder.Property(proveedor => proveedor.Direccion).HasMaxLength(300);
    }
}
