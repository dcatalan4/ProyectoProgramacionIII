using Microsoft.AspNetCore.Mvc;
using SuperBodega.Admin.Api.Dtos.Ventas;
using SuperBodega.Admin.Api.Services.Ventas;

namespace SuperBodega.Admin.Api.Controllers;

[ApiController]
[Route("api/ventas")]
public sealed class VentasController(IVentaService ventas) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<VentaResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await ventas.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VentaResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var venta = await ventas.GetByIdAsync(id, cancellationToken);
        return venta is null ? NotFound() : Ok(venta);
    }

    [HttpPost]
    public async Task<ActionResult<VentaResponse>> Create(CrearVentaRequest request, CancellationToken cancellationToken)
    {
        var result = await ventas.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VentaResponse>> Update(Guid id, ActualizarVentaRequest request, CancellationToken cancellationToken)
    {
        var result = await ventas.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPatch("{id:guid}/estado")]
    public async Task<ActionResult<VentaResponse>> CambiarEstado(Guid id, CambiarEstadoVentaRequest request, CancellationToken cancellationToken)
    {
        var result = await ventas.CambiarEstadoAsync(id, request, cancellationToken);
        return result.Success ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await ventas.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
