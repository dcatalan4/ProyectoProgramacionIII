namespace SuperBodega.Ecommerce.Api.Dtos.Catalogo;

public sealed record CatalogoProductoResponse(
    Guid Id,
    string Sku,
    string Nombre,
    string? Descripcion,
    decimal PrecioVenta,
    int Stock,
    Guid CategoriaId,
    string? Categoria);
