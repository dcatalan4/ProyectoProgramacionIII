using Microsoft.AspNetCore.Mvc;
using SuperBodega.Admin.Api.Dtos.Reportes;
using SuperBodega.Admin.Api.Services.Reportes;

namespace SuperBodega.Admin.Api.Controllers;

[ApiController]
[Route("api/reportes")]
public sealed class ReportesController(IReporteService reportes) : ControllerBase
{
    [HttpGet("periodo")]
    public async Task<ActionResult<ReportePeriodoResponse>> PorPeriodo(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken)
    {
        var result = await reportes.PorPeriodoAsync(desde, hasta, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("producto/{productoId}")]
    public async Task<ActionResult<ReporteProductoResponse>> PorProducto(
        string productoId,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken)
    {
        var result = await reportes.PorProductoAsync(productoId, desde, hasta, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<ReporteClienteResponse>> PorCliente(
        string clienteId,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken)
    {
        var result = await reportes.PorClienteAsync(clienteId, desde, hasta, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("proveedor/{proveedorId}")]
    public async Task<ActionResult<ReporteProveedorResponse>> PorProveedor(
        string proveedorId,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken)
    {
        var result = await reportes.PorProveedorAsync(proveedorId, desde, hasta, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }
}
