using SuperBodega.Admin.Api.Dtos.Proveedores;

namespace SuperBodega.Admin.Api.Services.Proveedores;

public interface IProveedorService
{
    Task<IReadOnlyCollection<ProveedorResponse>> GetAllAsync(bool incluirInactivos, CancellationToken cancellationToken);
    Task<ProveedorResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<ProveedorResponse>> CreateAsync(CrearProveedorRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ProveedorResponse>> UpdateAsync(Guid id, ActualizarProveedorRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
