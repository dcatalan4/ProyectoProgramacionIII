using Microsoft.EntityFrameworkCore;
using SuperBodega.Admin.Api.Dtos.Clientes;
using SuperBodega.Domain.Entities;
using SuperBodega.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace SuperBodega.Admin.Api.Services.Clientes;

public sealed class ClienteService(SuperBodegaDbContext dbContext) : IClienteService
{
    public async Task<IReadOnlyCollection<ClienteResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Apellido)
            .ThenBy(cliente => cliente.Nombre)
            .Select(cliente => ToResponse(cliente))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ClienteResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.Id == id)
            .Select(cliente => ToResponse(cliente))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<ClienteResponse>> CreateAsync(CrearClienteRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Id) || request.Id.Length < 4)
        {
            return ServiceResult<ClienteResponse>.Fail("El ID es obligatorio y debe tener al menos 4 caracteres.");
        }

        var id = StringToGuid(request.Id);

        var emailExists = await dbContext.Clientes.AnyAsync(cliente => cliente.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            return ServiceResult<ClienteResponse>.Fail("Ya existe un cliente con ese email.");
        }

        var cliente = new Cliente
        {
            Id = id,
            IdOriginal = request.Id,
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Email = request.Email.Trim(),
            Telefono = request.Telefono?.Trim(),
            DireccionEnvio = ResolverDireccion(request)
        };

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<ClienteResponse>.Ok(ToResponse(cliente));
    }

    public async Task<ServiceResult<ClienteResponse>> UpdateAsync(Guid id, ActualizarClienteRequest request, CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes.FindAsync([id], cancellationToken);
        if (cliente is null)
        {
            return ServiceResult<ClienteResponse>.Fail("Cliente no encontrado.");
        }

        var emailExists = await dbContext.Clientes.AnyAsync(item => item.Id != id && item.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            return ServiceResult<ClienteResponse>.Fail("Ya existe otro cliente con ese email.");
        }

        cliente.Nombre = request.Nombre.Trim();
        cliente.Apellido = request.Apellido.Trim();
        cliente.Email = request.Email.Trim();
        cliente.Telefono = request.Telefono?.Trim();
        cliente.DireccionEnvio = ResolverDireccion(request);
        cliente.IdOriginal = request.Id.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<ClienteResponse>.Ok(ToResponse(cliente));
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes.FindAsync([id], cancellationToken);
        if (cliente is null)
        {
            return false;
        }

        dbContext.Clientes.Remove(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ClienteResponse ToResponse(Cliente cliente)
    {
        var idOriginal = string.IsNullOrEmpty(cliente.IdOriginal)
            ? cliente.Id.ToString().Substring(0, Math.Min(8, cliente.Id.ToString().Length))
            : cliente.IdOriginal;

        return new ClienteResponse(
            cliente.Id,
            idOriginal,
            cliente.Nombre,
            cliente.Apellido,
            cliente.Email,
            cliente.Telefono,
            cliente.DireccionEnvio,
            cliente.FechaRegistroUtc);
    }

    private static string? ResolverDireccion(CrearClienteRequest request)
    {
        var direccion = request.DireccionEnvio ?? request.Direccion;
        return string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim();
    }

    private static Guid StringToGuid(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
