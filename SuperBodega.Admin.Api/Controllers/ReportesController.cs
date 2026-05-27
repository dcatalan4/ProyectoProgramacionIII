using Microsoft.AspNetCore.Mvc;
using SuperBodega.Admin.Api.Dtos.Reportes;
using SuperBodega.Admin.Api.Services.Reportes;
using System.Security.Cryptography;
using System.Text;

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
        var guid = StringToGuid(productoId);
        var result = await reportes.PorProductoAsync(guid, desde, hasta, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<ReporteClienteResponse>> PorCliente(
        string clienteId,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken)
    {
        var guid = StringToGuid(clienteId);
        var result = await reportes.PorClienteAsync(guid, desde, hasta, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("proveedor/{proveedorId}")]
    public async Task<ActionResult<ReporteProveedorResponse>> PorProveedor(
        string proveedorId,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken)
    {
        var guid = StringToGuid(proveedorId);
        var result = await reportes.PorProveedorAsync(guid, desde, hasta, cancellationToken);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }

    private static Guid StringToGuid(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
