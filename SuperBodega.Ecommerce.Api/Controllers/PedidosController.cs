using Microsoft.AspNetCore.Mvc;
using SuperBodega.Ecommerce.Api.Dtos.Pedidos;
using SuperBodega.Ecommerce.Api.Services.Pedidos;

namespace SuperBodega.Ecommerce.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public sealed class PedidosController(IPedidoService pedidos) : ControllerBase
{
    [HttpPost("sincrono")]
    public async Task<ActionResult<PedidoResponse>> CrearSincrono(CrearPedidoRequest request, CancellationToken cancellationToken)
    {
        var result = await pedidos.CrearSincronoAsync(request, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("asincrono")]
    public async Task<ActionResult<PedidoEncoladoResponse>> CrearAsincrono(CrearPedidoRequest request, CancellationToken cancellationToken)
    {
        var response = await pedidos.CrearAsincronoAsync(request, cancellationToken);
        return Accepted(response);
    }
}
