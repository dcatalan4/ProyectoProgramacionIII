using Microsoft.AspNetCore.Mvc;
using SuperBodega.Admin.Api.Dtos.Clientes;
using SuperBodega.Admin.Api.Services.Clientes;

namespace SuperBodega.Admin.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController(IClienteService clientes) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ClienteResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await clientes.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await clientes.GetByIdAsync(id, cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Create(CrearClienteRequest request, CancellationToken cancellationToken)
    {
        var result = await clientes.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> Update(Guid id, ActualizarClienteRequest request, CancellationToken cancellationToken)
    {
        var result = await clientes.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await clientes.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
