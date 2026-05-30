using Microsoft.EntityFrameworkCore;
using SuperBodega.Admin.Api.Dtos.Ventas;
using SuperBodega.Domain.Entities;
using SuperBodega.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace SuperBodega.Admin.Api.Services.Ventas;

public sealed class VentaService(SuperBodegaDbContext dbContext) : IVentaService
{
    public async Task<IReadOnlyCollection<VentaResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var ventas = await Query().OrderByDescending(venta => venta.FechaUtc).ToArrayAsync(cancellationToken);
        return ventas.Select(ToResponse).ToArray();
    }

    public async Task<VentaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var venta = await Query().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return venta is null ? null : ToResponse(venta);
    }

    public async Task<ServiceResult<VentaResponse>> CreateAsync(CrearVentaRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Id) || request.Id.Length < 4)
        {
            return ServiceResult<VentaResponse>.Fail("El ID es obligatorio y debe tener al menos 4 caracteres.");
        }

        var id = StringToGuid(request.Id);
        var clienteId = StringToGuid(request.ClienteId);

        var validation = await ValidateAsync(Guid.Empty, request, clienteId, null, cancellationToken);
        if (validation is not null)
        {
            return ServiceResult<VentaResponse>.Fail(validation);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var venta = new Venta
        {
            Id = id,
            IdOriginal = request.Id,
            NumeroVenta = request.NumeroVenta.Trim(),
            ClienteId = clienteId,
            FechaUtc = request.Fecha.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.Fecha, DateTimeKind.Utc)
                : request.Fecha.ToUniversalTime()
        };

        foreach (var requestedDetail in request.Detalles)
        {
            var productoId = StringToGuid(requestedDetail.ProductoId);
            var producto = await dbContext.Productos.FindAsync([productoId], cancellationToken);
            venta.Detalles.Add(new DetalleVenta
            {
                ProductoId = producto!.Id,
                Cantidad = requestedDetail.Cantidad,
                PrecioUnitario = producto.PrecioVenta
            });
        }

        await ReducirStockPorVentaAsync(venta.Detalles, cancellationToken);
        dbContext.Ventas.Add(venta);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<VentaResponse>.Ok((await GetByIdAsync(venta.Id, cancellationToken))!);
    }

    public async Task<ServiceResult<VentaResponse>> UpdateAsync(Guid id, ActualizarVentaRequest request, CancellationToken cancellationToken)
    {
        var venta = await dbContext.Ventas
            .Include(item => item.Detalles)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (venta is null)
        {
            return ServiceResult<VentaResponse>.Fail("Venta no encontrada.");
        }

        var clienteId = StringToGuid(request.ClienteId);

        var validation = await ValidateAsync(id, request, clienteId, venta.Detalles, cancellationToken);
        if (validation is not null)
        {
            return ServiceResult<VentaResponse>.Fail(validation);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await RestaurarStockPorVentaAsync(venta.Detalles, cancellationToken);

        dbContext.DetallesVenta.RemoveRange(venta.Detalles);
        venta.NumeroVenta = request.NumeroVenta.Trim();
        venta.ClienteId = clienteId;
        venta.Estado = request.Estado;
        venta.Detalles = [];

        foreach (var requestedDetail in request.Detalles)
        {
            var productoId = StringToGuid(requestedDetail.ProductoId);
            var producto = await dbContext.Productos.FindAsync([productoId], cancellationToken);
            venta.Detalles.Add(new DetalleVenta
            {
                VentaId = venta.Id,
                ProductoId = producto!.Id,
                Cantidad = requestedDetail.Cantidad,
                PrecioUnitario = producto.PrecioVenta
            });
        }

        await ReducirStockPorVentaAsync(venta.Detalles, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<VentaResponse>.Ok((await GetByIdAsync(venta.Id, cancellationToken))!);
    }

    public async Task<ServiceResult<VentaResponse>> CambiarEstadoAsync(Guid id, CambiarEstadoVentaRequest request, CancellationToken cancellationToken)
    {
        var venta = await dbContext.Ventas.FindAsync([id], cancellationToken);
        if (venta is null)
        {
            return ServiceResult<VentaResponse>.Fail("Venta no encontrada.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        venta.Estado = request.Estado;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<VentaResponse>.Ok((await GetByIdAsync(id, cancellationToken))!);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var venta = await dbContext.Ventas
            .Include(item => item.Detalles)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (venta is null)
        {
            return false;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await RestaurarStockPorVentaAsync(venta.Detalles, cancellationToken);

        dbContext.Ventas.Remove(venta);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    private IQueryable<Venta> Query()
    {
        return dbContext.Ventas
            .AsNoTracking()
            .Include(venta => venta.Cliente)
            .Include(venta => venta.Detalles)
            .ThenInclude(detalle => detalle.Producto);
    }

    private async Task<string?> ValidateAsync(
        Guid id,
        CrearVentaRequest request,
        Guid clienteId,
        IEnumerable<DetalleVenta>? detallesActuales,
        CancellationToken cancellationToken)
    {
        if (!request.Detalles.Any())
        {
            return "La venta debe incluir al menos un detalle.";
        }

        if (!await dbContext.Clientes.AnyAsync(cliente => cliente.Id == clienteId, cancellationToken))
        {
            return "El cliente no existe.";
        }

        if (await dbContext.Ventas.AnyAsync(venta => venta.Id != id && venta.NumeroVenta == request.NumeroVenta, cancellationToken))
        {
            return "Ya existe una venta con ese numero.";
        }

        var cantidadesActuales = detallesActuales?
            .GroupBy(detalle => detalle.ProductoId)
            .ToDictionary(group => group.Key, group => group.Sum(detalle => detalle.Cantidad))
            ?? [];

        foreach (var detalle in request.Detalles.GroupBy(detalle => detalle.ProductoId))
        {
            var productoIdString = detalle.Key;
            var productoId = StringToGuid(productoIdString);
            var cantidadSolicitada = detalle.Sum(item => item.Cantidad);
            var producto = await dbContext.Productos.FindAsync([productoId], cancellationToken);
            if (producto is null || !producto.EstaActivo)
            {
                return $"El producto {productoIdString} no existe o no esta activo.";
            }

            cantidadesActuales.TryGetValue(productoId, out var cantidadActual);
            var stockDisponible = producto.Stock + cantidadActual;
            if (cantidadSolicitada > stockDisponible)
            {
                return $"Stock insuficiente para {producto.Nombre}. Disponible: {stockDisponible}, solicitado: {cantidadSolicitada}.";
            }
        }

        return null;
    }

    private async Task ReducirStockPorVentaAsync(IEnumerable<DetalleVenta> detalles, CancellationToken cancellationToken)
    {
        foreach (var detalle in detalles.GroupBy(item => item.ProductoId))
        {
            var producto = await dbContext.Productos.FindAsync([detalle.Key], cancellationToken);
            producto!.Stock -= detalle.Sum(item => item.Cantidad);
        }
    }

    private async Task RestaurarStockPorVentaAsync(IEnumerable<DetalleVenta> detalles, CancellationToken cancellationToken)
    {
        foreach (var detalle in detalles.GroupBy(item => item.ProductoId))
        {
            var producto = await dbContext.Productos.FindAsync([detalle.Key], cancellationToken);
            if (producto is not null)
            {
                producto.Stock += detalle.Sum(item => item.Cantidad);
            }
        }
    }

    private static VentaResponse ToResponse(Venta venta)
    {
        var idOriginal = string.IsNullOrEmpty(venta.IdOriginal)
            ? venta.Id.ToString().Substring(0, Math.Min(8, venta.Id.ToString().Length))
            : venta.IdOriginal;

        var clienteId = !string.IsNullOrWhiteSpace(venta.Cliente?.IdOriginal)
            ? venta.Cliente!.IdOriginal
            : venta.ClienteId?.ToString() ?? string.Empty;

        return new VentaResponse(
            venta.Id,
            idOriginal,
            venta.NumeroVenta,
            venta.FechaUtc,
            venta.Estado,
            clienteId,
            venta.Cliente is null ? null : $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}",
            venta.Total,
            venta.Detalles.Select(detalle =>
            {
                var productoId = !string.IsNullOrWhiteSpace(detalle.Producto?.IdOriginal)
                    ? detalle.Producto!.IdOriginal
                    : detalle.ProductoId.ToString();

                return new DetalleVentaResponse(
                    detalle.Id,
                    productoId,
                    detalle.Producto?.Nombre,
                    detalle.Cantidad,
                    detalle.PrecioUnitario,
                    detalle.Subtotal);
            }).ToArray());
    }

    private static Guid StringToGuid(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
