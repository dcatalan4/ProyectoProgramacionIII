using System.ComponentModel.DataAnnotations;

namespace SuperBodega.Admin.Api.Dtos.Productos;

public sealed record ProductoResponse(
    Guid Id,
    string IdOriginal,
    string Nombre,
    string? Descripcion,
    decimal PrecioVenta,
    decimal PrecioCompra,
    int Stock,
    bool EstaActivo,
    string ProveedorId,
    string? Proveedor);

public class CrearProductoRequest
{
    [Required, MinLength(4)]
    public string Id { get; set; } = string.Empty;

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

    [Required, MinLength(4)]
    public string ProveedorId { get; set; } = string.Empty;
}

public sealed class ActualizarProductoRequest : CrearProductoRequest
{
    public bool EstaActivo { get; set; } = true;
}
