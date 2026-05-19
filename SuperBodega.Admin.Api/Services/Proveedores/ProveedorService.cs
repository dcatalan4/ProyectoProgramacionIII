using Microsoft.EntityFrameworkCore;
using SuperBodega.Admin.Api.Dtos.Proveedores;
using SuperBodega.Domain.Entities;
using SuperBodega.Infrastructure.Data;

namespace SuperBodega.Admin.Api.Services.Proveedores;

public sealed class ProveedorService(SuperBodegaDbContext dbContext) : IProveedorService
{
    public async Task<IReadOnlyCollection<ProveedorResponse>> GetAllAsync(bool incluirInactivos, CancellationToken cancellationToken)
    {
        return await dbContext.Proveedores
            .AsNoTracking()
            .Where(proveedor => incluirInactivos || proveedor.EstaActivo)
            .OrderBy(proveedor => proveedor.Nombre)
            .Select(proveedor => ToResponse(proveedor))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ProveedorResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Proveedores
            .AsNoTracking()
            .Where(proveedor => proveedor.Id == id)
            .Select(proveedor => ToResponse(proveedor))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<ProveedorResponse>> CreateAsync(CrearProveedorRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateUniqueAsync(Guid.Empty, request.Nit, request.Email, cancellationToken);
        if (validation is not null)
        {
            return ServiceResult<ProveedorResponse>.Fail(validation);
        }

        var proveedor = new Proveedor
        {
            Nombre = request.Nombre.Trim(),
            Nit = request.Nit?.Trim(),
            Telefono = request.Telefono?.Trim(),
            Email = request.Email?.Trim(),
            Direccion = request.Direccion?.Trim()
        };

        dbContext.Proveedores.Add(proveedor);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<ProveedorResponse>.Ok(ToResponse(proveedor));
    }

    public async Task<ServiceResult<ProveedorResponse>> UpdateAsync(Guid id, ActualizarProveedorRequest request, CancellationToken cancellationToken)
    {
        var proveedor = await dbContext.Proveedores.FindAsync([id], cancellationToken);
        if (proveedor is null)
        {
            return ServiceResult<ProveedorResponse>.Fail("Proveedor no encontrado.");
        }

        var validation = await ValidateUniqueAsync(id, request.Nit, request.Email, cancellationToken);
        if (validation is not null)
        {
            return ServiceResult<ProveedorResponse>.Fail(validation);
        }

        proveedor.Nombre = request.Nombre.Trim();
        proveedor.Nit = request.Nit?.Trim();
        proveedor.Telefono = request.Telefono?.Trim();
        proveedor.Email = request.Email?.Trim();
        proveedor.Direccion = request.Direccion?.Trim();
        proveedor.EstaActivo = request.EstaActivo;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<ProveedorResponse>.Ok(ToResponse(proveedor));
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var proveedor = await dbContext.Proveedores.FindAsync([id], cancellationToken);
        if (proveedor is null)
        {
            return false;
        }

        proveedor.EstaActivo = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string?> ValidateUniqueAsync(Guid id, string? nit, string? email, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(nit) &&
            await dbContext.Proveedores.AnyAsync(proveedor => proveedor.Id != id && proveedor.Nit == nit, cancellationToken))
        {
            return "Ya existe un proveedor con ese NIT.";
        }

        if (!string.IsNullOrWhiteSpace(email) &&
            await dbContext.Proveedores.AnyAsync(proveedor => proveedor.Id != id && proveedor.Email == email, cancellationToken))
        {
            return "Ya existe un proveedor con ese email.";
        }

        return null;
    }

    private static ProveedorResponse ToResponse(Proveedor proveedor)
    {
        return new ProveedorResponse(
            proveedor.Id,
            proveedor.Nombre,
            proveedor.Nit,
            proveedor.Telefono,
            proveedor.Email,
            proveedor.Direccion,
            proveedor.EstaActivo);
    }
}
