using Microsoft.EntityFrameworkCore;
using SuperBodega.Admin.Api.Dtos.Categorias;
using SuperBodega.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace SuperBodega.Admin.Api.Services.Categorias;

public sealed class CategoriaService(SuperBodegaDbContext dbContext) : ICategoriaService
{
    public async Task<IReadOnlyCollection<CategoriaResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categorias = await dbContext.Categorias
            .AsNoTracking()
            .Select(c => new CategoriaResponse(
                c.Id,
                c.IdOriginal,
                c.Nombre,
                c.Descripcion,
                c.EstaActiva))
            .ToListAsync(cancellationToken);

        return categorias;
    }

    public async Task<CategoriaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var categoria = await dbContext.Categorias
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoriaResponse(
                c.Id,
                c.IdOriginal,
                c.Nombre,
                c.Descripcion,
                c.EstaActiva))
            .FirstOrDefaultAsync(cancellationToken);

        return categoria;
    }

    public async Task<ServiceResult<CategoriaResponse>> CreateAsync(CrearCategoriaRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Id) || request.Id.Length < 4)
        {
            return ServiceResult<CategoriaResponse>.Fail("El ID es obligatorio y debe tener al menos 4 caracteres.");
        }

        var id = StringToGuid(request.Id);

        var categoria = new Domain.Entities.Categoria
        {
            Id = id,
            IdOriginal = request.Id,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            EstaActiva = true
        };

        dbContext.Categorias.Add(categoria);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CategoriaResponse(
            categoria.Id,
            categoria.IdOriginal,
            categoria.Nombre,
            categoria.Descripcion,
            categoria.EstaActiva);

        return ServiceResult<CategoriaResponse>.Ok(response);
    }

    public async Task<ServiceResult<CategoriaResponse>> UpdateAsync(Guid id, ActualizarCategoriaRequest request, CancellationToken cancellationToken)
    {
        var categoria = await dbContext.Categorias.FindAsync([id], cancellationToken);
        if (categoria is null)
        {
            return ServiceResult<CategoriaResponse>.Fail("Categoría no encontrada.");
        }

        categoria.Nombre = request.Nombre;
        categoria.Descripcion = request.Descripcion;
        categoria.EstaActiva = request.EstaActivo;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CategoriaResponse(
            categoria.Id,
            categoria.IdOriginal,
            categoria.Nombre,
            categoria.Descripcion,
            categoria.EstaActiva);

        return ServiceResult<CategoriaResponse>.Ok(response);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var categoria = await dbContext.Categorias.FindAsync([id], cancellationToken);
        if (categoria is null)
        {
            return false;
        }

        dbContext.Categorias.Remove(categoria);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static Guid StringToGuid(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
