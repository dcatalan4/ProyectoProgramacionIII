using Microsoft.EntityFrameworkCore;
using SuperBodega.Ecommerce.Api.Dtos.Catalogo;
using SuperBodega.Ecommerce.Api.Dtos.Common;
using SuperBodega.Infrastructure.Data;

namespace SuperBodega.Ecommerce.Api.Services.Catalogo;

public sealed class CatalogoService(SuperBodegaDbContext dbContext) : ICatalogoService
{
    public async Task<PagedResponse<CatalogoProductoResponse>> GetAsync(
        int page,
        int pageSize,
        Guid? categoriaId,
        CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Productos
            .AsNoTracking()
            .Include(producto => producto.Categoria)
            .Where(producto => producto.EstaActivo &&
                               (!categoriaId.HasValue || producto.CategoriaId == categoriaId));

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(producto => producto.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(producto => new CatalogoProductoResponse(
                producto.Id,
                producto.Sku,
                producto.Nombre,
                producto.Descripcion,
                producto.PrecioVenta,
                producto.Stock,
                producto.CategoriaId,
                producto.Categoria == null ? null : producto.Categoria.Nombre))
            .ToArrayAsync(cancellationToken);

        return new PagedResponse<CatalogoProductoResponse>(
            page,
            pageSize,
            totalItems,
            (int)Math.Ceiling(totalItems / (double)pageSize),
            items);
    }
}
