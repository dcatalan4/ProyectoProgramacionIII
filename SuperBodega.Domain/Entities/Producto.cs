namespace SuperBodega.Domain.Entities;

public sealed class Producto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string IdOriginal { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal PrecioCompra { get; set; }
    public int Stock { get; set; }
    public bool EstaActivo { get; set; } = true;
    public Guid? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public Guid ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    public ICollection<DetalleCompra> DetallesCompra { get; set; } = [];
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = [];
    public ICollection<CarritoDetalle> CarritoDetalles { get; set; } = [];
}
