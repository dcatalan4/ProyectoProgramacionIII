using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBodega.Ecommerce.Api.Dtos.Pedidos;
using SuperBodega.Ecommerce.Api.Services.Pedidos;
using SuperBodega.Infrastructure.Data;
using SuperBodega.Domain.Entities;

namespace SuperBodega.Ecommerce.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public sealed class PedidosController(IPedidoService pedidos, SuperBodegaDbContext dbContext) : ControllerBase
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

    [HttpGet("solicitud/{solicitudId}")]
    public async Task<ActionResult> ObtenerEstadoSolicitud(Guid solicitudId, CancellationToken cancellationToken)
    {
        var solicitud = await dbContext.SolicitudPedidos
            .FirstOrDefaultAsync(s => s.Id == solicitudId, cancellationToken);

        if (solicitud is null)
        {
            return NotFound("Solicitud no encontrada");
        }

        return Ok(new
        {
            solicitud.Id,
            solicitud.CarritoId,
            solicitud.Estado,
            solicitud.CreadoUtc,
            solicitud.ProcesadoUtc,
            solicitud.MensajeError,
            solicitud.VentaId
        });
    }

    [HttpPost("solicitud/{solicitudId}/finalizar")]
    public async Task<ActionResult<PedidoResponse>> FinalizarSolicitudManualmente(Guid solicitudId, CancellationToken cancellationToken)
    {
        var solicitud = await dbContext.SolicitudPedidos
            .FirstOrDefaultAsync(s => s.Id == solicitudId, cancellationToken);

        if (solicitud is null)
        {
            return NotFound("Solicitud no encontrada");
        }

        if (solicitud.Estado == EstadoSolicitud.Completado)
        {
            return BadRequest("La solicitud ya fue completada");
        }

        solicitud.Estado = EstadoSolicitud.Procesando;
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = await pedidos.ProcesarCarritoAsync(solicitud.CarritoId, cancellationToken);

        if (result.Success)
        {
            solicitud.Estado = EstadoSolicitud.Completado;
            solicitud.ProcesadoUtc = DateTime.UtcNow;
            solicitud.VentaId = result.Value.VentaId;
        }
        else
        {
            solicitud.Estado = EstadoSolicitud.Fallido;
            solicitud.ProcesadoUtc = DateTime.UtcNow;
            solicitud.MensajeError = result.Error;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }
}
