using SuperBodega.Admin.Api.Dtos.Compras;

namespace SuperBodega.Admin.Api.Services.Compras;

public interface ICompraService
{
    Task<IReadOnlyCollection<CompraResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<CompraResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<CompraResponse>> CreateAsync(CrearCompraRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CompraResponse>> UpdateAsync(Guid id, ActualizarCompraRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
