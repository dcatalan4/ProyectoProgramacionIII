using System.ComponentModel.DataAnnotations;

namespace SuperBodega.Admin.Api.Dtos.Productos;

public sealed record ProductoResponse(
    Guid Id,
    string Sku,
    string Nombre,
    string? Descripcion,
    decimal PrecioVenta,
    decimal PrecioCompra,
    int Stock,
    bool EstaActivo,
    Guid CategoriaId,
    string? Categoria,
    Guid ProveedorId,
    string? Proveedor);

public class CrearProductoRequest
{
    [Required, MaxLength(40)]
    public string Sku { get; set; } = string.Empty;

    [Required, MaxLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal PrecioVenta { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal PrecioCompra { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required]
    public Guid CategoriaId { get; set; }

    [Required]
    public Guid ProveedorId { get; set; }
}

public sealed class ActualizarProductoRequest : CrearProductoRequest
{
    public bool EstaActivo { get; set; } = true;
}
