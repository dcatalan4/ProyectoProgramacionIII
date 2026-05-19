using SuperBodega.Admin.Api.Dtos.Reportes;

namespace SuperBodega.Admin.Api.Services.Reportes;

public interface IReporteService
{
    Task<ServiceResult<ReportePeriodoResponse>> PorPeriodoAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken);
    Task<ServiceResult<ReporteProductoResponse>> PorProductoAsync(Guid productoId, DateTime desde, DateTime hasta, CancellationToken cancellationToken);
    Task<ServiceResult<ReporteClienteResponse>> PorClienteAsync(Guid clienteId, DateTime desde, DateTime hasta, CancellationToken cancellationToken);
    Task<ServiceResult<ReporteProveedorResponse>> PorProveedorAsync(Guid proveedorId, DateTime desde, DateTime hasta, CancellationToken cancellationToken);
}
