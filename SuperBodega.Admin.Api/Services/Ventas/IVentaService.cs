using SuperBodega.Admin.Api.Dtos.Ventas;

namespace SuperBodega.Admin.Api.Services.Ventas;

public interface IVentaService
{
    Task<IReadOnlyCollection<VentaResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<VentaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<VentaResponse>> CreateAsync(CrearVentaRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<VentaResponse>> UpdateAsync(Guid id, ActualizarVentaRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<VentaResponse>> CambiarEstadoAsync(Guid id, CambiarEstadoVentaRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
