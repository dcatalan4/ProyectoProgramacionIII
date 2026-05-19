namespace SuperBodega.Domain.Entities;

public sealed class Categoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EstaActiva { get; set; } = true;
    public ICollection<Producto> Productos { get; set; } = [];
}
