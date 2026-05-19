using SuperBodega.Admin.Api.Dtos.Productos;

namespace SuperBodega.Admin.Api.Services.Productos;

public interface IProductoService
{
    Task<IReadOnlyCollection<ProductoResponse>> GetAllAsync(bool incluirInactivos, CancellationToken cancellationToken);
    Task<ProductoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<ProductoResponse>> CreateAsync(CrearProductoRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ProductoResponse>> UpdateAsync(Guid id, ActualizarProductoRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
