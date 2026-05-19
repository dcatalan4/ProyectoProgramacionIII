using System.ComponentModel.DataAnnotations;

namespace SuperBodega.Admin.Api.Dtos.Proveedores;

public sealed record ProveedorResponse(
    Guid Id,
    string Nombre,
    string? Nit,
    string? Telefono,
    string? Email,
    string? Direccion,
    bool EstaActivo);

public class CrearProveedorRequest
{
    [Required, MaxLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Nit { get; set; }

    [MaxLength(30)]
    public string? Telefono { get; set; }

    [EmailAddress, MaxLength(160)]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }
}

public sealed class ActualizarProveedorRequest : CrearProveedorRequest
{
    public bool EstaActivo { get; set; } = true;
}
