using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");
        builder.HasKey(cliente => cliente.Id);
        builder.Property(cliente => cliente.Nombre).HasMaxLength(120).IsRequired();
        builder.Property(cliente => cliente.Apellido).HasMaxLength(120).IsRequired();
        builder.Property(cliente => cliente.Email).HasMaxLength(160).IsRequired();
        builder.Property(cliente => cliente.Telefono).HasMaxLength(30);
        builder.Property(cliente => cliente.DireccionEnvio).HasMaxLength(300);
        builder.HasIndex(cliente => cliente.Email).IsUnique();
    }
}
