namespace SuperBodega.Domain.Entities;

public sealed class Venta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarritoId { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
    public EstadoVenta Estado { get; set; } = EstadoVenta.Recibida;
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public ICollection<DetalleVenta> Detalles { get; set; } = [];
    public ICollection<NotificacionPedido> NotificacionesPedido { get; set; } = [];
    public decimal Total => Detalles.Sum(detalle => detalle.Subtotal);
}
