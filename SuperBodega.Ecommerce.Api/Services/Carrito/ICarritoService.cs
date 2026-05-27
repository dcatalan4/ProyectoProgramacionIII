using SuperBodega.Ecommerce.Api.Dtos.Carrito;

namespace SuperBodega.Ecommerce.Api.Services.Carrito;

public interface ICarritoService
{
    Task<CarritoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<CarritoResponse>> CreateAsync(CrearCarritoRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CarritoResponse>> AddItemAsync(AgregarCarritoItemRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CarritoResponse>> UpdateItemAsync(ActualizarCarritoItemRequest request, CancellationToken cancellationToken);
}
