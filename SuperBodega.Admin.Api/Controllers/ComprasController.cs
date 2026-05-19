using Microsoft.AspNetCore.Mvc;
using SuperBodega.Admin.Api.Dtos.Compras;
using SuperBodega.Admin.Api.Services.Compras;

namespace SuperBodega.Admin.Api.Controllers;

[ApiController]
[Route("api/compras")]
public sealed class ComprasController(ICompraService compras) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CompraResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await compras.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompraResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var compra = await compras.GetByIdAsync(id, cancellationToken);
        return compra is null ? NotFound() : Ok(compra);
    }

    [HttpPost]
    public async Task<ActionResult<CompraResponse>> Create(CrearCompraRequest request, CancellationToken cancellationToken)
    {
        var result = await compras.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CompraResponse>> Update(Guid id, ActualizarCompraRequest request, CancellationToken cancellationToken)
    {
        var result = await compras.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await compras.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
