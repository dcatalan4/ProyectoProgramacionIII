namespace SuperBodega.Ecommerce.Api.Dtos.Catalogo;

public sealed record CatalogoProductoResponse(
    Guid id,
    string idOriginal,
    string sku,
    string nombre,
    string? descripcion,
    decimal precioVenta,
    int stock,
    Guid? categoriaId,
    string? categoria);
