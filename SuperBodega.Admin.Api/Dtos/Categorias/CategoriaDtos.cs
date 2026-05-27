using System.ComponentModel.DataAnnotations;

namespace SuperBodega.Admin.Api.Dtos.Categorias;

public sealed record CategoriaResponse(
    Guid Id,
    string IdOriginal,
    string Nombre,
    string? Descripcion,
    bool EstaActivo);

public class CrearCategoriaRequest
{
    [Required, MinLength(4)]
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }
}

public sealed class ActualizarCategoriaRequest : CrearCategoriaRequest
{
    public bool EstaActivo { get; set; } = true;
}
