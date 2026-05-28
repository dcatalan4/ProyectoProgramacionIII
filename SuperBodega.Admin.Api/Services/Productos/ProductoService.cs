using Microsoft.EntityFrameworkCore;
using SuperBodega.Admin.Api.Dtos.Productos;
using SuperBodega.Domain.Entities;
using SuperBodega.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

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
        var productoId = StringToGuid(request.Id);
        var proveedorId = await ResolveProveedorIdAsync(request.ProveedorId, cancellationToken);
        if (proveedorId is null)
        {
            return ServiceResult<ProductoResponse>.Fail("El proveedor no existe.");
        }

        var producto = new Producto
        {
            Id = productoId,
            IdOriginal = request.Id.Trim(),
            Sku = Guid.NewGuid().ToString(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            PrecioVenta = request.PrecioVenta,
            PrecioCompra = request.PrecioCompra,
            Stock = request.Stock,
            CategoriaId = null,
            ProveedorId = proveedorId.Value
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

        var proveedorId = await ResolveProveedorIdAsync(request.ProveedorId, cancellationToken);
        if (proveedorId is null)
        {
            return ServiceResult<ProductoResponse>.Fail("El proveedor no existe.");
        }

        producto.Nombre = request.Nombre.Trim();
        producto.IdOriginal = request.Id.Trim();
        producto.Descripcion = request.Descripcion?.Trim();
        producto.PrecioVenta = request.PrecioVenta;
        producto.PrecioCompra = request.PrecioCompra;
        producto.Stock = request.Stock;
        producto.EstaActivo = request.EstaActivo;
        producto.ProveedorId = proveedorId.Value;

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

    public async Task<int> BackfillIdOriginalesAsync(CancellationToken cancellationToken)
    {
        var productos = await dbContext.Productos
            .Where(producto => string.IsNullOrWhiteSpace(producto.IdOriginal))
            .ToListAsync(cancellationToken);

        var actualizados = 0;

        foreach (var producto in productos)
        {
            var recuperado = TryRecoverIdOriginal(producto.Id, producto.Nombre, producto.Sku);
            if (string.IsNullOrWhiteSpace(recuperado))
            {
                continue;
            }

            producto.IdOriginal = recuperado;
            actualizados++;
        }

        if (actualizados > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return actualizados;
    }

    private async Task<Guid?> ResolveProveedorIdAsync(string proveedorId, CancellationToken cancellationToken)
    {
        var id = proveedorId.Trim();
        var parsed = Guid.TryParse(id, out var guid) ? guid : StringToGuid(id);

        var proveedor = await dbContext.Proveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == parsed || item.IdOriginal == id, cancellationToken);

        if (proveedor is null && !Guid.TryParse(id, out _))
        {
            proveedor = await dbContext.Proveedores
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == StringToGuid(id), cancellationToken);
        }

        return proveedor?.Id;
    }

    private static ProductoResponse ToResponse(Producto producto)
    {
        return new ProductoResponse(
            producto.Id,
            producto.IdOriginal,
            producto.Nombre,
            producto.Descripcion,
            producto.PrecioVenta,
            producto.PrecioCompra,
            producto.Stock,
            producto.EstaActivo,
            !string.IsNullOrWhiteSpace(producto.Proveedor?.IdOriginal)
                ? producto.Proveedor!.IdOriginal
                : producto.ProveedorId.ToString(),
            producto.Proveedor?.Nombre);
    }

    private static Guid StringToGuid(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    private static string? TryRecoverIdOriginal(Guid id, string nombre, string sku)
    {
        var candidatos = new HashSet<string>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var nombreLimpio = nombre.Trim();
            candidatos.Add(nombreLimpio);
            candidatos.Add(nombreLimpio.ToLowerInvariant());
            candidatos.Add(nombreLimpio.ToUpperInvariant());
            candidatos.Add(nombreLimpio.Replace(" ", "", StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(sku) &&
            sku.Length >= 4 &&
            !Guid.TryParse(sku, out _))
        {
            candidatos.Add(sku.Trim());
        }

        foreach (var candidato in candidatos)
        {
            if (candidato.Length < 4)
            {
                continue;
            }

            if (StringToGuid(candidato) == id)
            {
                return candidato;
            }
        }

        for (var numero = 1000; numero <= 999_999; numero++)
        {
            if (StringToGuid(numero.ToString()) == id)
            {
                return numero.ToString();
            }
        }

        return null;
    }
}
