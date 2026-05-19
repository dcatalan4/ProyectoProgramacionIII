namespace SuperBodega.Domain.Entities;

public sealed class CarritoDetalle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarritoId { get; set; }
    public Carrito? Carrito { get; set; }
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
