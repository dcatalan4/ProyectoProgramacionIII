using Microsoft.EntityFrameworkCore;
using SuperBodega.Admin.Api.Dtos.Clientes;
using SuperBodega.Domain.Entities;
using SuperBodega.Infrastructure.Data;

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
        var emailExists = await dbContext.Clientes.AnyAsync(cliente => cliente.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            return ServiceResult<ClienteResponse>.Fail("Ya existe un cliente con ese email.");
        }

        var cliente = new Cliente
        {
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Email = request.Email.Trim(),
            Telefono = request.Telefono?.Trim(),
            DireccionEnvio = request.DireccionEnvio?.Trim()
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
        cliente.DireccionEnvio = request.DireccionEnvio?.Trim();

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
        return new ClienteResponse(
            cliente.Id,
            cliente.Nombre,
            cliente.Apellido,
            cliente.Email,
            cliente.Telefono,
            cliente.DireccionEnvio,
            cliente.FechaRegistroUtc);
    }
}
