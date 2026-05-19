using Microsoft.EntityFrameworkCore;
using SuperBodega.Admin.Api.Dtos.Productos;
using SuperBodega.Domain.Entities;
using SuperBodega.Infrastructure.Data;

namespace SuperBodega.Admin.Api.Services.Productos;

public sealed class ProductoService(SuperBodegaDbContext dbContext) : IProductoService
{
    public async Task<IReadOnlyCollection<ProductoResponse>> GetAllAsync(bool incluirInactivos, CancellationToken cancellationToken)
    {
        return await dbContext.Productos
            .AsNoTracking()
            .Include(producto => producto.Categoria)
            .Include(producto => producto.Proveedor)
            .Where(producto => incluirInactivos || producto.EstaActivo)
            .OrderBy(producto => producto.Nombre)
            .Select(producto => ToResponse(producto))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ProductoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Productos
            .AsNoTracking()
            .Include(producto => producto.Categoria)
            .Include(producto => producto.Proveedor)
            .Where(producto => producto.Id == id)
            .Select(producto => ToResponse(producto))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<ProductoResponse>> CreateAsync(CrearProductoRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateReferencesAsync(request.CategoriaId, request.ProveedorId, cancellationToken);
        if (validation is not null)
        {
            return ServiceResult<ProductoResponse>.Fail(validation);
        }

        var skuExists = await dbContext.Productos.AnyAsync(producto => producto.Sku == request.Sku, cancellationToken);
        if (skuExists)
        {
            return ServiceResult<ProductoResponse>.Fail("Ya existe un producto con ese SKU.");
        }

        var producto = new Producto
        {
            Sku = request.Sku.Trim(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            PrecioVenta = request.PrecioVenta,
            PrecioCompra = request.PrecioCompra,
            Stock = request.Stock,
            CategoriaId = request.CategoriaId,
            ProveedorId = request.ProveedorId
        };

        dbContext.Productos.Add(producto);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductoResponse>.Ok((await GetByIdAsync(producto.Id, cancellationToken))!);
    }

    public async Task<ServiceResult<ProductoResponse>> UpdateAsync(Guid id, ActualizarProductoRequest request, CancellationToken cancellationToken)
    {
        var producto = await dbContext.Productos.FindAsync([id], cancellationToken);
        if (producto is null)
        {
            return ServiceResult<ProductoResponse>.Fail("Producto no encontrado.");
        }

        var validation = await ValidateReferencesAsync(request.CategoriaId, request.ProveedorId, cancellationToken);
        if (validation is not null)
        {
            return ServiceResult<ProductoResponse>.Fail(validation);
        }

        var skuExists = await dbContext.Productos.AnyAsync(item => item.Id != id && item.Sku == request.Sku, cancellationToken);
        if (skuExists)
        {
            return ServiceResult<ProductoResponse>.Fail("Ya existe otro producto con ese SKU.");
        }

        producto.Sku = request.Sku.Trim();
        producto.Nombre = request.Nombre.Trim();
        producto.Descripcion = request.Descripcion?.Trim();
        producto.PrecioVenta = request.PrecioVenta;
        producto.PrecioCompra = request.PrecioCompra;
        producto.Stock = request.Stock;
        producto.EstaActivo = request.EstaActivo;
        producto.CategoriaId = request.CategoriaId;
        producto.ProveedorId = request.ProveedorId;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<ProductoResponse>.Ok((await GetByIdAsync(producto.Id, cancellationToken))!);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var producto = await dbContext.Productos.FindAsync([id], cancellationToken);
        if (producto is null)
        {
            return false;
        }

        producto.EstaActivo = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string?> ValidateReferencesAsync(Guid categoriaId, Guid proveedorId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Categorias.AnyAsync(categoria => categoria.Id == categoriaId, cancellationToken))
        {
            return "La categoria no existe.";
        }

        if (!await dbContext.Proveedores.AnyAsync(proveedor => proveedor.Id == proveedorId, cancellationToken))
        {
            return "El proveedor no existe.";
        }

        return null;
    }

    private static ProductoResponse ToResponse(Producto producto)
    {
        return new ProductoResponse(
            producto.Id,
            producto.Sku,
            producto.Nombre,
            producto.Descripcion,
            producto.PrecioVenta,
            producto.PrecioCompra,
            producto.Stock,
            producto.EstaActivo,
            producto.CategoriaId,
            producto.Categoria?.Nombre,
            producto.ProveedorId,
            producto.Proveedor?.Nombre);
    }
}
