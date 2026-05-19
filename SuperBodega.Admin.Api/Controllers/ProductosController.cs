using Microsoft.AspNetCore.Mvc;
using SuperBodega.Admin.Api.Dtos.Productos;
using SuperBodega.Admin.Api.Services.Productos;

namespace SuperBodega.Admin.Api.Controllers;

[ApiController]
[Route("api/productos")]
public sealed class ProductosController(IProductoService productos) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductoResponse>>> GetAll(
        [FromQuery] bool incluirInactivos = true,
        CancellationToken cancellationToken = default)
    {
        return Ok(await productos.GetAllAsync(incluirInactivos, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductoResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var producto = await productos.GetByIdAsync(id, cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductoResponse>> Create(CrearProductoRequest request, CancellationToken cancellationToken)
    {
        var result = await productos.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductoResponse>> Update(Guid id, ActualizarProductoRequest request, CancellationToken cancellationToken)
    {
        var result = await productos.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await productos.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
