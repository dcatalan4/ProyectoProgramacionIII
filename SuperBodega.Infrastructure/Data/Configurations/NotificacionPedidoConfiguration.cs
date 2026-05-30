using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Infrastructure.Data.Configurations;

public sealed class NotificacionPedidoConfiguration : IEntityTypeConfiguration<NotificacionPedido>
{
    public void Configure(EntityTypeBuilder<NotificacionPedido> builder)
    {
        builder.ToTable("notificaciones_pedido");
        builder.HasKey(notificacion => notificacion.Id);
        builder.Property(notificacion => notificacion.Destinatario).HasMaxLength(160).IsRequired();
        builder.Property(notificacion => notificacion.Mensaje).HasMaxLength(1000).IsRequired();

        builder.HasOne(notificacion => notificacion.Venta)
            .WithMany(venta => venta.NotificacionesPedido)
            .HasForeignKey(notificacion => notificacion.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(notificacion => notificacion.Cliente)
            .WithMany(cliente => cliente.NotificacionesPedido)
            .HasForeignKey(notificacion => notificacion.ClienteId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
