namespace SuperBodega.Domain.Entities;

public sealed class DetalleCompra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompraId { get; set; }
    public Compra? Compra { get; set; }
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal => Cantidad * CostoUnitario;
}
