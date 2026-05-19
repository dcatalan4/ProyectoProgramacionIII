using SuperBodega.Ecommerce.Api.Dtos.Pedidos;

namespace SuperBodega.Ecommerce.Api.Services.Pedidos;

public interface IPedidoService
{
    Task<ServiceResult<PedidoResponse>> CrearSincronoAsync(CrearPedidoRequest request, CancellationToken cancellationToken);
    Task<PedidoEncoladoResponse> CrearAsincronoAsync(CrearPedidoRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<PedidoResponse>> ProcesarCarritoAsync(Guid carritoId, CancellationToken cancellationToken);
}
