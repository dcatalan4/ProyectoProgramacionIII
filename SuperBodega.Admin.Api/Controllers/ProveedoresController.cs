using Microsoft.AspNetCore.Mvc;
using SuperBodega.Admin.Api.Dtos.Proveedores;
using SuperBodega.Admin.Api.Services.Proveedores;

namespace SuperBodega.Admin.Api.Controllers;

[ApiController]
[Route("api/proveedores")]
public sealed class ProveedoresController(IProveedorService proveedores) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProveedorResponse>>> GetAll(
        [FromQuery] bool incluirInactivos = true,
        CancellationToken cancellationToken = default)
    {
        return Ok(await proveedores.GetAllAsync(incluirInactivos, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProveedorResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var proveedor = await proveedores.GetByIdAsync(id, cancellationToken);
        return proveedor is null ? NotFound() : Ok(proveedor);
    }

    [HttpPost]
    public async Task<ActionResult<ProveedorResponse>> Create(CrearProveedorRequest request, CancellationToken cancellationToken)
    {
        var result = await proveedores.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProveedorResponse>> Update(Guid id, ActualizarProveedorRequest request, CancellationToken cancellationToken)
    {
        var result = await proveedores.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await proveedores.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
