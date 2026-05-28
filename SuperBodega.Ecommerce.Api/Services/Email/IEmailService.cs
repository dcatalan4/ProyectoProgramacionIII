namespace SuperBodega.Ecommerce.Api.Services.Email;

public interface IEmailService
{
    Task<EmailSendResult> SendPedidoConfirmationAsync(
        PedidoEmailMessage pedido,
        CancellationToken cancellationToken);
}
