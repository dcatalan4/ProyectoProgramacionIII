using Microsoft.AspNetCore.Mvc;
using SuperBodega.Ecommerce.Api.Dtos.Catalogo;
using SuperBodega.Ecommerce.Api.Dtos.Common;
using SuperBodega.Ecommerce.Api.Services.Catalogo;

namespace SuperBodega.Ecommerce.Api.Controllers;

[ApiController]
[Route("api/catalogo")]
public sealed class CatalogoController(ICatalogoService catalogo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CatalogoProductoResponse>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoriaId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await catalogo.GetAsync(page, pageSize, categoriaId, cancellationToken));
    }
}
