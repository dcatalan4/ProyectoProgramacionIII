using Microsoft.AspNetCore.Mvc;
using SuperBodega.Admin.Api.Dtos.Categorias;
using SuperBodega.Admin.Api.Services.Categorias;

namespace SuperBodega.Admin.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public sealed class CategoriasController(ICategoriaService categorias) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CategoriaResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await categorias.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoriaResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var categoria = await categorias.GetByIdAsync(id, cancellationToken);
        return categoria is null ? NotFound() : Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaResponse>> Create(CrearCategoriaRequest request, CancellationToken cancellationToken)
    {
        var result = await categorias.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoriaResponse>> Update(Guid id, ActualizarCategoriaRequest request, CancellationToken cancellationToken)
    {
        var result = await categorias.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await categorias.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
