namespace SuperBodega.Domain.Entities;

public sealed class Carrito
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public EstadoCarrito Estado { get; set; } = EstadoCarrito.Abierto;
    public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ActualizadoUtc { get; set; }
    public ICollection<CarritoDetalle> Detalles { get; set; } = [];
    public decimal Total => Detalles.Sum(detalle => detalle.Subtotal);
}
