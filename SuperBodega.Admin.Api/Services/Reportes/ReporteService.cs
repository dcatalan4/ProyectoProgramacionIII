using Microsoft.EntityFrameworkCore;
using SuperBodega.Admin.Api.Dtos.Reportes;
using SuperBodega.Infrastructure.Data;

namespace SuperBodega.Admin.Api.Services.Reportes;

public sealed class ReporteService(SuperBodegaDbContext dbContext) : IReporteService
{
    public async Task<ServiceResult<ReportePeriodoResponse>> PorPeriodoAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken)
    {
        var validation = ValidatePeriod(desde, hasta);
        if (validation is not null)
        {
            return ServiceResult<ReportePeriodoResponse>.Fail(validation);
        }

        // Convertir DateTime a UTC para PostgreSQL
        var desdeUtc = desde.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(desde, DateTimeKind.Utc) : desde.ToUniversalTime();
        var hastaUtc = hasta.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(hasta, DateTimeKind.Utc) : hasta.ToUniversalTime();

        var ventas = await dbContext.Ventas
            .AsNoTracking()
            .Include(venta => venta.Detalles)
            .Where(venta => venta.FechaUtc >= desdeUtc && venta.FechaUtc <= hastaUtc)
            .ToArrayAsync(cancellationToken);

        var compras = await dbContext.Compras
            .AsNoTracking()
            .Include(compra => compra.Detalles)
            .Where(compra => compra.FechaUtc >= desdeUtc && compra.FechaUtc <= hastaUtc)
            .ToArrayAsync(cancellationToken);

        return ServiceResult<ReportePeriodoResponse>.Ok(new ReportePeriodoResponse(
            desde,
            hasta,
            ventas.Length,
            ventas.Sum(venta => venta.Total),
            compras.Length,
            compras.Sum(compra => compra.Total)));
    }

    public async Task<ServiceResult<ReporteProductoResponse>> PorProductoAsync(Guid productoId, DateTime desde, DateTime hasta, CancellationToken cancellationToken)
    {
        var validation = ValidatePeriod(desde, hasta);
        if (validation is not null)
        {
            return ServiceResult<ReporteProductoResponse>.Fail(validation);
        }

        // Convertir DateTime a UTC para PostgreSQL
        var desdeUtc = desde.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(desde, DateTimeKind.Utc) : desde.ToUniversalTime();
        var hastaUtc = hasta.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(hasta, DateTimeKind.Utc) : hasta.ToUniversalTime();

        var producto = await dbContext.Productos.AsNoTracking().FirstOrDefaultAsync(item => item.Id == productoId, cancellationToken);
        if (producto is null)
        {
            return ServiceResult<ReporteProductoResponse>.Fail("Producto no encontrado.");
        }

        var ventas = await dbContext.DetallesVenta
            .AsNoTracking()
            .Include(detalle => detalle.Venta)
            .Where(detalle => detalle.ProductoId == productoId &&
                              detalle.Venta != null &&
                              detalle.Venta.FechaUtc >= desdeUtc &&
                              detalle.Venta.FechaUtc <= hastaUtc)
            .ToArrayAsync(cancellationToken);

        var compras = await dbContext.DetallesCompra
            .AsNoTracking()
            .Include(detalle => detalle.Compra)
            .Where(detalle => detalle.ProductoId == productoId &&
                              detalle.Compra != null &&
                              detalle.Compra.FechaUtc >= desdeUtc &&
                              detalle.Compra.FechaUtc <= hastaUtc)
            .ToArrayAsync(cancellationToken);

        return ServiceResult<ReporteProductoResponse>.Ok(new ReporteProductoResponse(
            producto.Id,
            producto.Nombre,
            ventas.Sum(detalle => detalle.Cantidad),
            ventas.Sum(detalle => detalle.Subtotal),
            compras.Sum(detalle => detalle.Cantidad),
            compras.Sum(detalle => detalle.Subtotal)));
    }

    public async Task<ServiceResult<ReporteClienteResponse>> PorClienteAsync(Guid clienteId, DateTime desde, DateTime hasta, CancellationToken cancellationToken)
    {
        var validation = ValidatePeriod(desde, hasta);
        if (validation is not null)
        {
            return ServiceResult<ReporteClienteResponse>.Fail(validation);
        }

        // Convertir DateTime a UTC para PostgreSQL
        var desdeUtc = desde.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(desde, DateTimeKind.Utc) : desde.ToUniversalTime();
        var hastaUtc = hasta.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(hasta, DateTimeKind.Utc) : hasta.ToUniversalTime();

        var cliente = await dbContext.Clientes.AsNoTracking().FirstOrDefaultAsync(item => item.Id == clienteId, cancellationToken);
        if (cliente is null)
        {
            return ServiceResult<ReporteClienteResponse>.Fail("Cliente no encontrado.");
        }

        var ventas = await dbContext.Ventas
            .AsNoTracking()
            .Include(venta => venta.Detalles)
            .Where(venta => venta.ClienteId == clienteId && venta.FechaUtc >= desdeUtc && venta.FechaUtc <= hastaUtc)
            .ToArrayAsync(cancellationToken);

        return ServiceResult<ReporteClienteResponse>.Ok(new ReporteClienteResponse(
            cliente.Id,
            $"{cliente.Nombre} {cliente.Apellido}",
            ventas.Length,
            ventas.Sum(venta => venta.Total)));
    }

    public async Task<ServiceResult<ReporteProveedorResponse>> PorProveedorAsync(Guid proveedorId, DateTime desde, DateTime hasta, CancellationToken cancellationToken)
    {
        var validation = ValidatePeriod(desde, hasta);
        if (validation is not null)
        {
            return ServiceResult<ReporteProveedorResponse>.Fail(validation);
        }

        // Convertir DateTime a UTC para PostgreSQL
        var desdeUtc = desde.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(desde, DateTimeKind.Utc) : desde.ToUniversalTime();
        var hastaUtc = hasta.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(hasta, DateTimeKind.Utc) : hasta.ToUniversalTime();

        var proveedor = await dbContext.Proveedores.AsNoTracking().FirstOrDefaultAsync(item => item.Id == proveedorId, cancellationToken);
        if (proveedor is null)
        {
            return ServiceResult<ReporteProveedorResponse>.Fail("Proveedor no encontrado.");
        }

        var compras = await dbContext.Compras
            .AsNoTracking()
            .Include(compra => compra.Detalles)
            .Where(compra => compra.ProveedorId == proveedorId && compra.FechaUtc >= desdeUtc && compra.FechaUtc <= hastaUtc)
            .ToArrayAsync(cancellationToken);

        var productosActivos = await dbContext.Productos
            .CountAsync(producto => producto.ProveedorId == proveedorId && producto.EstaActivo, cancellationToken);

        return ServiceResult<ReporteProveedorResponse>.Ok(new ReporteProveedorResponse(
            proveedor.Id,
            proveedor.Nombre,
            compras.Length,
            compras.Sum(compra => compra.Total),
            productosActivos));
    }

    private static string? ValidatePeriod(DateTime desde, DateTime hasta)
    {
        if (desde > hasta)
        {
            return "La fecha desde no puede ser mayor que la fecha hasta.";
        }

        return null;
    }
}
