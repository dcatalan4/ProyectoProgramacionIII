using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");
        builder.HasKey(categoria => categoria.Id);
        builder.Property(categoria => categoria.Nombre).HasMaxLength(120).IsRequired();
        builder.Property(categoria => categoria.Descripcion).HasMaxLength(300);
        builder.HasIndex(categoria => categoria.Nombre).IsUnique();
    }
}
