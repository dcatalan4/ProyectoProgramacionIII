using System.ComponentModel.DataAnnotations;

namespace SuperBodega.Admin.Api.Dtos.Clientes;

public sealed record ClienteResponse(
    Guid Id,
    string IdOriginal,
    string Nombre,
    string Apellido,
    string Email,
    string? Telefono,
    string? DireccionEnvio,
    DateTime FechaRegistroUtc);

public class CrearClienteRequest
{
    [Required, MinLength(4)]
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Apellido { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Telefono { get; set; }

    [MaxLength(300)]
    public string? DireccionEnvio { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }
}

public sealed class ActualizarClienteRequest : CrearClienteRequest;
