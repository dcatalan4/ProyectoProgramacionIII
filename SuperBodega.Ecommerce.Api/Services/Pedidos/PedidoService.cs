using Microsoft.EntityFrameworkCore;

using SuperBodega.Domain.Entities;

using SuperBodega.Ecommerce.Api.Dtos.Pedidos;

using SuperBodega.Ecommerce.Api.Messaging;

using SuperBodega.Ecommerce.Api.Services.Email;

using SuperBodega.Infrastructure.Data;



namespace SuperBodega.Ecommerce.Api.Services.Pedidos;



public sealed class PedidoService(
    SuperBodegaDbContext dbContext,
    KafkaProducer kafkaProducer,
    IEmailService emailService) : IPedidoService

{

    public Task<ServiceResult<PedidoResponse>> CrearSincronoAsync(

        CrearPedidoRequest request,

        CancellationToken cancellationToken)

    {

        return ProcesarCarritoAsync(

            request.CarritoId,

            cancellationToken);

    }



    public async Task<PedidoEncoladoResponse> CrearAsincronoAsync(

        CrearPedidoRequest request,

        CancellationToken cancellationToken)

    {

        var solicitudId = Guid.NewGuid();



        await kafkaProducer.EnviarPedidoAsync(request.CarritoId);



        return new PedidoEncoladoResponse(

            solicitudId,

            request.CarritoId,

            "Encolado");

    }



    public async Task<ServiceResult<PedidoResponse>> ProcesarCarritoAsync(

        Guid carritoId,

        CancellationToken cancellationToken)

    {

        await using var transaction = await dbContext.Database

            .BeginTransactionAsync(cancellationToken);



        var carrito = await dbContext.Carritos

            .Include(item => item.Cliente)

            .Include(item => item.Detalles)

            .ThenInclude(detalle => detalle.Producto)

            .FirstOrDefaultAsync(

                item => item.Id == carritoId,

                cancellationToken);



        if (carrito is null)

        {

            return ServiceResult<PedidoResponse>

                .Fail("El carrito no existe.");

        }



        if (carrito.Estado == EstadoCarrito.Cerrado)

        {

            var ventaExistente = await dbContext.Ventas

                .Include(v => v.NotificacionesPedido)

                .FirstOrDefaultAsync(v => v.CarritoId == carritoId, cancellationToken);



            if (ventaExistente != null)

            {

                var notif = ventaExistente.NotificacionesPedido.First();



                return ServiceResult<PedidoResponse>

                    .Ok(ToResponse(ventaExistente, carrito, notif));

            }



            return ServiceResult<PedidoResponse>

                .Fail("Carrito cerrado sin venta.");

        }



        if (!carrito.Detalles.Any())

        {

            return ServiceResult<PedidoResponse>

                .Fail("El carrito no tiene items.");

        }

        if (!carrito.ClienteId.HasValue)

        {

            return ServiceResult<PedidoResponse>

                .Fail("El carrito debe estar asociado a un cliente.");

        }



        var stockValidation = ValidateStock(carrito);



        if (stockValidation is not null)

        {

            return ServiceResult<PedidoResponse>

                .Fail(stockValidation);

        }



        var venta = new Venta

        {

            ClienteId = carrito.ClienteId,

            CarritoId = carrito.Id,

            NumeroVenta = $"P-{DateTime.UtcNow:yyyyMMddHHmmssfff}",

            Estado = EstadoVenta.Recibida

        };



        foreach (var item in carrito.Detalles)

        {

            item.Producto!.Stock -= item.Cantidad;



            venta.Detalles.Add(new DetalleVenta

            {

                ProductoId = item.ProductoId,

                Cantidad = item.Cantidad,

                PrecioUnitario = item.PrecioUnitario

            });

        }



        var notificacion = CrearNotificacion(venta, carrito);



        venta.NotificacionesPedido.Add(notificacion);



        carrito.Estado = EstadoCarrito.Cerrado;



        carrito.ActualizadoUtc = DateTime.UtcNow;



        dbContext.Ventas.Add(venta);



        Console.WriteLine("Guardando en base de datos...");



        await dbContext.SaveChangesAsync(cancellationToken);



        Console.WriteLine(" Guardado en DB");



        await transaction.CommitAsync(cancellationToken);



        Console.WriteLine("Transacción completada");

        await EnviarCorreoPedidoAsync(venta, carrito, notificacion, cancellationToken);



        return ServiceResult<PedidoResponse>

            .Ok(ToResponse(venta, carrito, notificacion));

    }



    private static string? ValidateStock(

        SuperBodega.Domain.Entities.Carrito carrito)

    {

        foreach (var itemGroup in carrito.Detalles.GroupBy(item => item.ProductoId))

        {

            var item = itemGroup.First();



            var producto = item.Producto;



            var cantidadSolicitada = itemGroup.Sum(

                detalle => detalle.Cantidad);



            if (producto is null || !producto.EstaActivo)

            {

                return $"El producto {item.ProductoId} no existe o no esta activo.";

            }



            if (cantidadSolicitada > producto.Stock)

            {

                return $"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}, solicitado: {cantidadSolicitada}.";

            }

        }



        return null;

    }



    private static NotificacionPedido CrearNotificacion(

        Venta venta,

        SuperBodega.Domain.Entities.Carrito carrito)

    {

        var destinatario =

            carrito.Cliente?.Email ?? "cliente@sin-email.local";



        var mensaje =

            $"Pedido {venta.NumeroVenta} recibido por SuperBodega. Total: Q{carrito.Total:0.00}.";



        return new NotificacionPedido

        {

            Venta = venta,

            ClienteId = carrito.ClienteId,

            Tipo = TipoNotificacionPedido.Email,

            Destinatario = destinatario,

            Mensaje = mensaje,

            FueEnviada = false,

            EnviadaUtc = null

        };

    }



    private async Task EnviarCorreoPedidoAsync(
        Venta venta,
        SuperBodega.Domain.Entities.Carrito carrito,
        NotificacionPedido notificacion,
        CancellationToken cancellationToken)
    {
        if (carrito.Cliente is null)
        {
            notificacion.FueEnviada = false;
            notificacion.Mensaje = $"{notificacion.Mensaje} Correo no enviado: cliente no disponible.";
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var email = new PedidoEmailMessage(
            venta.NumeroVenta,
            venta.FechaUtc,
            carrito.Cliente,
            venta.Total,
            carrito.Detalles.Select(detalle => new PedidoEmailDetalle(
                detalle.Producto?.Nombre ?? detalle.ProductoId.ToString(),
                detalle.Cantidad,
                detalle.PrecioUnitario,
                detalle.Subtotal)).ToArray());

        var result = await emailService.SendPedidoConfirmationAsync(email, cancellationToken);

        notificacion.FueEnviada = result.Success;
        notificacion.EnviadaUtc = result.Success ? DateTime.UtcNow : null;
        notificacion.Mensaje = $"{notificacion.Mensaje} {result.Message}";

        await dbContext.SaveChangesAsync(cancellationToken);
    }



    private static PedidoResponse ToResponse(

        Venta venta,

        SuperBodega.Domain.Entities.Carrito carrito,

        NotificacionPedido notificacion)

    {

        return new PedidoResponse(

            venta.Id,

            venta.NumeroVenta,

            venta.Estado,

            venta.ClienteId,

            venta.Total,

            new PedidoNotificacionResponse(

                notificacion.Id,

                notificacion.Destinatario,

                notificacion.Mensaje,

                notificacion.FueEnviada,

                notificacion.EnviadaUtc),

            carrito.Detalles.Select(item =>

                new PedidoDetalleResponse(

                    item.ProductoId,

                    item.Producto?.Nombre,

                    item.Cantidad,

                    item.PrecioUnitario,

                    item.Subtotal))

            .ToArray());

    }

}
