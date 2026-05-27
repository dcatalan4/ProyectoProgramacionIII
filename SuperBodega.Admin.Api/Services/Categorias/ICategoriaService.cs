using SuperBodega.Admin.Api.Dtos.Categorias;

namespace SuperBodega.Admin.Api.Services.Categorias;

public interface ICategoriaService
{
    Task<IReadOnlyCollection<CategoriaResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<CategoriaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<CategoriaResponse>> CreateAsync(CrearCategoriaRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CategoriaResponse>> UpdateAsync(Guid id, ActualizarCategoriaRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
