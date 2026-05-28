using SuperBodega.Admin.Api.Dtos.Reportes;

namespace SuperBodega.Admin.Api.Services.Reportes;

public interface IReporteService
{
    Task<ServiceResult<ReportePeriodoResponse>> PorPeriodoAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken);
    Task<ServiceResult<ReporteProductoResponse>> PorProductoAsync(string productoId, DateTime desde, DateTime hasta, CancellationToken cancellationToken);
    Task<ServiceResult<ReporteClienteResponse>> PorClienteAsync(string clienteId, DateTime desde, DateTime hasta, CancellationToken cancellationToken);
    Task<ServiceResult<ReporteProveedorResponse>> PorProveedorAsync(string proveedorId, DateTime desde, DateTime hasta, CancellationToken cancellationToken);
}
