using Microsoft.AspNetCore.Mvc;
using SuperBodega.Ecommerce.Api.Dtos.Carrito;
using SuperBodega.Ecommerce.Api.Services.Carrito;

namespace SuperBodega.Ecommerce.Api.Controllers;

[ApiController]
[Route("api/carrito")]
public sealed class CarritoController(ICarritoService carrito) : ControllerBase
{
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
}
