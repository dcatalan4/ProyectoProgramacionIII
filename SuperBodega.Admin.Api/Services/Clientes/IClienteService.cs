using SuperBodega.Admin.Api.Dtos.Clientes;

namespace SuperBodega.Admin.Api.Services.Clientes;

public interface IClienteService
{
    Task<IReadOnlyCollection<ClienteResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<ClienteResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<ClienteResponse>> CreateAsync(CrearClienteRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ClienteResponse>> UpdateAsync(Guid id, ActualizarClienteRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
