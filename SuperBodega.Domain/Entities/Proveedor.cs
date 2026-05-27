namespace SuperBodega.Domain.Entities;

public sealed class Proveedor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string IdOriginal { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Nit { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public bool EstaActivo { get; set; } = true;
    public ICollection<Producto> Productos { get; set; } = [];
    public ICollection<Compra> Compras { get; set; } = [];
}
