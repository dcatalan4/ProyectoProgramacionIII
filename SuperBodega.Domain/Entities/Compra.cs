namespace SuperBodega.Domain.Entities;

public sealed class Compra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NumeroCompra { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
    public Guid ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    public ICollection<DetalleCompra> Detalles { get; set; } = [];
    public decimal Total => Detalles.Sum(detalle => detalle.Subtotal);
}
