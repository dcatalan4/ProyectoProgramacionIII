using Microsoft.EntityFrameworkCore;
using SuperBodega.Admin.Api.Dtos.Compras;
using SuperBodega.Domain.Entities;
using SuperBodega.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace SuperBodega.Admin.Api.Services.Compras;

public sealed class CompraService(SuperBodegaDbContext dbContext) : ICompraService
{
    public async Task<IReadOnlyCollection<CompraResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var compras = await Query().OrderByDescending(compra => compra.FechaUtc).ToArrayAsync(cancellationToken);
        return compras.Select(ToResponse).ToArray();
    }

    public async Task<CompraResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var compra = await Query().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return compra is null ? null : ToResponse(compra);
    }

    public async Task<ServiceResult<CompraResponse>> CreateAsync(CrearCompraRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Id) || request.Id.Length < 4)
        {
            return ServiceResult<CompraResponse>.Fail("El ID es obligatorio y debe tener al menos 4 caracteres.");
        }

        var id = StringToGuid(request.Id);
        var proveedorId = StringToGuid(request.ProveedorId);

        var validation = await ValidateAsync(Guid.Empty, request, proveedorId, cancellationToken);
        if (validation is not null)
        {
            return ServiceResult<CompraResponse>.Fail(validation);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var compra = new Compra
        {
            Id = id,
            IdOriginal = request.Id,
            NumeroCompra = request.NumeroCompra.Trim(),
            ProveedorId = proveedorId,
            FechaUtc = ParseFechaUtc(request.Fecha),
            Detalles = request.Detalles.Select(detalle => new DetalleCompra
            {
                ProductoId = StringToGuid(detalle.ProductoId),
                Cantidad = detalle.Cantidad,
                CostoUnitario = detalle.CostoUnitario
            }).ToList()
        };

        await AumentarStockPorCompraAsync(compra.Detalles, cancellationToken);

        dbContext.Compras.Add(compra);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<CompraResponse>.Ok((await GetByIdAsync(compra.Id, cancellationToken))!);
    }

    public async Task<ServiceResult<CompraResponse>> UpdateAsync(Guid id, ActualizarCompraRequest request, CancellationToken cancellationToken)
    {
        var compra = await dbContext.Compras
            .Include(item => item.Detalles)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (compra is null)
        {
            return ServiceResult<CompraResponse>.Fail("Compra no encontrada.");
        }

        var proveedorId = StringToGuid(request.ProveedorId);

        var validation = await ValidateAsync(id, request, proveedorId, cancellationToken);
        if (validation is not null)
        {
            return ServiceResult<CompraResponse>.Fail(validation);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var rollbackError = await RevertirStockPorCompraAsync(compra.Detalles, cancellationToken);
        if (rollbackError is not null)
        {
            return ServiceResult<CompraResponse>.Fail(rollbackError);
        }

        dbContext.DetallesCompra.RemoveRange(compra.Detalles);
        compra.NumeroCompra = request.NumeroCompra.Trim();
        compra.ProveedorId = proveedorId;
        compra.FechaUtc = ParseFechaUtc(request.Fecha);
        compra.Detalles = request.Detalles.Select(detalle => new DetalleCompra
        {
            CompraId = compra.Id,
            ProductoId = StringToGuid(detalle.ProductoId),
            Cantidad = detalle.Cantidad,
            CostoUnitario = detalle.CostoUnitario
        }).ToList();

        await AumentarStockPorCompraAsync(compra.Detalles, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<CompraResponse>.Ok((await GetByIdAsync(compra.Id, cancellationToken))!);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var compra = await dbContext.Compras
            .Include(item => item.Detalles)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (compra is null)
        {
            return false;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var rollbackError = await RevertirStockPorCompraAsync(compra.Detalles, cancellationToken);
        if (rollbackError is not null)
        {
            return false;
        }

        dbContext.Compras.Remove(compra);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    private IQueryable<Compra> Query()
    {
        return dbContext.Compras
            .AsNoTracking()
            .Include(compra => compra.Proveedor)
            .Include(compra => compra.Detalles)
            .ThenInclude(detalle => detalle.Producto);
    }

    private async Task<string?> ValidateAsync(Guid id, CrearCompraRequest request, Guid proveedorId, CancellationToken cancellationToken)
    {
        if (!request.Detalles.Any())
        {
            return "La compra debe incluir al menos un detalle.";
        }

        if (!await dbContext.Proveedores.AnyAsync(proveedor => proveedor.Id == proveedorId, cancellationToken))
        {
            return "El proveedor no existe.";
        }

        if (await dbContext.Compras.AnyAsync(compra => compra.Id != id && compra.NumeroCompra == request.NumeroCompra, cancellationToken))
        {
            return "Ya existe una compra con ese numero.";
        }

        var productIds = request.Detalles.Select(detalle => StringToGuid(detalle.ProductoId)).Distinct().ToArray();
        var existingProducts = await dbContext.Productos.CountAsync(producto => productIds.Contains(producto.Id), cancellationToken);
        return existingProducts == productIds.Length ? null : "Uno o mas productos no existen.";
    }

    private async Task AumentarStockPorCompraAsync(IEnumerable<DetalleCompra> detalles, CancellationToken cancellationToken)
    {
        foreach (var detalle in detalles)
        {
            var producto = await dbContext.Productos.FindAsync([detalle.ProductoId], cancellationToken);
            producto!.Stock += detalle.Cantidad;
            producto.PrecioCompra = detalle.CostoUnitario;
        }
    }

    private async Task<string?> RevertirStockPorCompraAsync(IEnumerable<DetalleCompra> detalles, CancellationToken cancellationToken)
    {
        foreach (var detalle in detalles)
        {
            var producto = await dbContext.Productos.FindAsync([detalle.ProductoId], cancellationToken);
            if (producto is null)
            {
                return $"El producto {detalle.ProductoId} no existe.";
            }

            if (producto.Stock < detalle.Cantidad)
            {
                return $"No se puede revertir la compra porque el stock de {producto.Nombre} quedaria negativo.";
            }

            producto.Stock -= detalle.Cantidad;
        }

        return null;
    }

    private static CompraResponse ToResponse(Compra compra)
    {
        var idOriginal = string.IsNullOrEmpty(compra.IdOriginal)
            ? compra.Id.ToString().Substring(0, Math.Min(8, compra.Id.ToString().Length))
            : compra.IdOriginal;

        var proveedorId = !string.IsNullOrWhiteSpace(compra.Proveedor?.IdOriginal)
            ? compra.Proveedor!.IdOriginal
            : compra.ProveedorId.ToString();

        return new CompraResponse(
            compra.Id,
            idOriginal,
            compra.NumeroCompra,
            compra.FechaUtc,
            proveedorId,
            compra.Proveedor?.Nombre,
            compra.Total,
            compra.Detalles.Select(detalle =>
            {
                var productoId = !string.IsNullOrWhiteSpace(detalle.Producto?.IdOriginal)
                    ? detalle.Producto!.IdOriginal
                    : detalle.ProductoId.ToString();

                return new DetalleCompraResponse(
                    detalle.Id,
                    productoId,
                    detalle.Producto?.Nombre,
                    detalle.Cantidad,
                    detalle.CostoUnitario,
                    detalle.Subtotal);
            }).ToArray());
    }

    private static Guid StringToGuid(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    private static DateTime ParseFechaUtc(DateTime fecha)
    {
        return fecha.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
            : fecha.ToUniversalTime();
    }
}
