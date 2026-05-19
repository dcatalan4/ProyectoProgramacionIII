using SuperBodega.Ecommerce.Api.Dtos.Catalogo;
using SuperBodega.Ecommerce.Api.Dtos.Common;

namespace SuperBodega.Ecommerce.Api.Services.Catalogo;

public interface ICatalogoService
{
    Task<PagedResponse<CatalogoProductoResponse>> GetAsync(
        int page,
        int pageSize,
        Guid? categoriaId,
        CancellationToken cancellationToken);
}
