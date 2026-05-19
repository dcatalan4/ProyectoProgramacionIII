using Microsoft.EntityFrameworkCore;
using SuperBodega.Domain.Entities;
using SuperBodega.Ecommerce.Api.Dtos.Carrito;
using SuperBodega.Infrastructure.Data;

namespace SuperBodega.Ecommerce.Api.Services.Carrito;

public sealed class CarritoService(SuperBodegaDbContext dbContext) : ICarritoService
{
    public async Task<ServiceResult<CarritoResponse>> CreateAsync(CrearCarritoRequest request, CancellationToken cancellationToken)
    {
        var clienteExists = await dbContext.Clientes.AnyAsync(cliente => cliente.Id == request.ClienteId, cancellationToken);
        if (!clienteExists)
        {
            return ServiceResult<CarritoResponse>.Fail("El cliente no existe.");
        }

        var carrito = new SuperBodega.Domain.Entities.Carrito
        {
            ClienteId = request.ClienteId
        };

        dbContext.Carritos.Add(carrito);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<CarritoResponse>.Ok((await GetResponseAsync(carrito.Id, cancellationToken))!);
    }

    public async Task<ServiceResult<CarritoResponse>> AddItemAsync(AgregarCarritoItemRequest request, CancellationToken cancellationToken)
    {
        var carrito = await dbContext.Carritos
            .Include(item => item.Detalles)
            .FirstOrDefaultAsync(item => item.Id == request.CarritoId, cancellationToken);
        if (carrito is null)
        {
            return ServiceResult<CarritoResponse>.Fail("El carrito no existe.");
        }

        if (carrito.Estado != EstadoCarrito.Abierto)
        {
            return ServiceResult<CarritoResponse>.Fail("El carrito no esta abierto.");
        }

        var producto = await dbContext.Productos.FindAsync([request.ProductoId], cancellationToken);
        if (producto is null || !producto.EstaActivo)
        {
            return ServiceResult<CarritoResponse>.Fail("El producto no existe o no esta activo.");
        }

        if (request.Cantidad > producto.Stock)
        {
            return ServiceResult<CarritoResponse>.Fail($"Stock insuficiente para {producto.Nombre}.");
        }

        var existingItem = carrito.Detalles.FirstOrDefault(item => item.ProductoId == request.ProductoId);
        if (existingItem is null)
        {
            carrito.Detalles.Add(new CarritoDetalle
            {
                ProductoId = producto.Id,
                Cantidad = request.Cantidad,
                PrecioUnitario = producto.PrecioVenta
            });
        }
        else
        {
            var nuevaCantidad = existingItem.Cantidad + request.Cantidad;
            if (nuevaCantidad > producto.Stock)
            {
                return ServiceResult<CarritoResponse>.Fail($"Stock insuficiente para {producto.Nombre}.");
            }

            existingItem.Cantidad = nuevaCantidad;
            existingItem.PrecioUnitario = producto.PrecioVenta;
        }

        carrito.ActualizadoUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<CarritoResponse>.Ok((await GetResponseAsync(carrito.Id, cancellationToken))!);
    }

    private async Task<CarritoResponse?> GetResponseAsync(Guid carritoId, CancellationToken cancellationToken)
    {
        var carrito = await dbContext.Carritos
            .AsNoTracking()
            .Include(item => item.Detalles)
            .ThenInclude(detalle => detalle.Producto)
            .FirstOrDefaultAsync(item => item.Id == carritoId, cancellationToken);

        return carrito is null
            ? null
            : new CarritoResponse(
                carrito.Id,
                carrito.ClienteId,
                carrito.Estado,
                carrito.CreadoUtc,
                carrito.ActualizadoUtc,
                carrito.Total,
                carrito.Detalles.Select(detalle => new CarritoItemResponse(
                    detalle.Id,
                    detalle.ProductoId,
                    detalle.Producto?.Nombre,
                    detalle.Cantidad,
                    detalle.PrecioUnitario,
                    detalle.Subtotal)).ToArray());
    }
}
