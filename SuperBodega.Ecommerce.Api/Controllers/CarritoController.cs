using Microsoft.AspNetCore.Mvc;
using SuperBodega.Ecommerce.Api.Dtos.Carrito;
using SuperBodega.Ecommerce.Api.Services.Carrito;

namespace SuperBodega.Ecommerce.Api.Controllers;

[ApiController]
[Route("api/carrito")]
public sealed class CarritoController(ICarritoService carrito) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<CarritoResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await carrito.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CarritoResponse>> Create(CrearCarritoRequest request, CancellationToken cancellationToken)
    {
        var result = await carrito.CreateAsync(request, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CarritoResponse>> AddItem(AgregarCarritoItemRequest request, CancellationToken cancellationToken)
    {
        var result = await carrito.AddItemAsync(request, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("items")]
    public async Task<ActionResult<CarritoResponse>> UpdateItem(ActualizarCarritoItemRequest request, CancellationToken cancellationToken)
    {
        var result = await carrito.UpdateItemAsync(request, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }
}
