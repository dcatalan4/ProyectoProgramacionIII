using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;

namespace SuperBodega.Ecommerce.Api.Services.Email;

public sealed class MailjetEmailService(HttpClient httpClient, IConfiguration configuration) : IEmailService
{
    private static readonly CultureInfo GuatemalaCulture = CultureInfo.GetCultureInfo("es-GT");

    public async Task<EmailSendResult> SendPedidoConfirmationAsync(
        PedidoEmailMessage pedido,
        CancellationToken cancellationToken)
    {
        var apiKey = configuration["Mailjet:ApiKey"];
        var secretKey = configuration["Mailjet:SecretKey"];
        var fromEmail = configuration["Mailjet:FromEmail"];
        var fromName = configuration["Mailjet:FromName"] ?? "SuperBodega";

        if (string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(secretKey) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            return new EmailSendResult(false, "Envio de correo no configurado.");
        }

        if (string.IsNullOrWhiteSpace(pedido.Cliente.Email))
        {
            return new EmailSendResult(false, "El cliente no tiene email configurado.");
        }

        var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiKey}:{secretKey}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

        var payload = new
        {
            Messages = new[]
            {
                new
                {
                    From = new
                    {
                        Email = fromEmail,
                        Name = fromName
                    },
                    To = new[]
                    {
                        new
                        {
                            Email = pedido.Cliente.Email,
                            Name = $"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}".Trim()
                        }
                    },
                    Subject = $"Detalle de tu pedido {pedido.NumeroVenta}",
                    TextPart = BuildText(pedido),
                    HTMLPart = BuildHtml(pedido)
                }
            }
        };

        using var response = await httpClient.PostAsJsonAsync(
            "https://api.mailjet.com/v3.1/send",
            payload,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return new EmailSendResult(true, "Correo enviado por Mailjet.");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new EmailSendResult(false, $"Mailjet respondio {(int)response.StatusCode}: {body}");
    }

    private static string BuildText(PedidoEmailMessage pedido)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Pedido: {pedido.NumeroVenta}");
        builder.AppendLine($"Fecha: {pedido.FechaUtc:yyyy-MM-dd HH:mm} UTC");
        builder.AppendLine();
        builder.AppendLine("Cliente:");
        builder.AppendLine($"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}");
        builder.AppendLine($"Email: {pedido.Cliente.Email}");
        builder.AppendLine($"Telefono: {pedido.ClienteTelefono()}");
        builder.AppendLine($"Direccion: {pedido.Cliente.DireccionEnvio ?? "No registrada"}");
        builder.AppendLine();
        builder.AppendLine("Detalle:");

        foreach (var detalle in pedido.Detalles)
        {
            builder.AppendLine(
                $"- {detalle.Producto}: {detalle.Cantidad} x {FormatMoney(detalle.PrecioUnitario)} = {FormatMoney(detalle.Subtotal)}");
        }

        builder.AppendLine();
        builder.AppendLine($"Total: {FormatMoney(pedido.Total)}");
        return builder.ToString();
    }

    private static string BuildHtml(PedidoEmailMessage pedido)
    {
        var clienteNombre = Html($"{pedido.Cliente.Nombre} {pedido.Cliente.Apellido}".Trim());
        var direccion = Html(pedido.Cliente.DireccionEnvio ?? "No registrada");
        var telefono = Html(pedido.ClienteTelefono());
        var fecha = Html($"{pedido.FechaUtc:yyyy-MM-dd HH:mm} UTC");

        var rows = string.Join(
            "",
            pedido.Detalles.Select(detalle => $"""
                <tr style="background-color:#112240;">
                    <td style="padding:14px 16px;border-bottom:1px solid #233554;color:#e8f4f8;font-size:14px;">{Html(detalle.Producto)}</td>
                    <td style="padding:14px 16px;border-bottom:1px solid #233554;color:#e8f4f8;font-size:14px;text-align:right;">{detalle.Cantidad}</td>
                    <td style="padding:14px 16px;border-bottom:1px solid #233554;color:#e8f4f8;font-size:14px;text-align:right;">{FormatMoney(detalle.PrecioUnitario)}</td>
                    <td style="padding:14px 16px;border-bottom:1px solid #233554;color:#64ffda;font-size:14px;font-weight:bold;text-align:right;">{FormatMoney(detalle.Subtotal)}</td>
                </tr>
                """));

        return $"""
            <div style="margin:0;padding:0;background-color:#0a192f;font-family:Arial,Helvetica,sans-serif;color:#e8f4f8;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:collapse;background-color:#0a192f;">
                    <tr>
                        <td align="center" style="padding:32px 14px;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:collapse;width:100%;max-width:720px;background-color:#112240;border:1px solid #233554;border-radius:10px;overflow:hidden;">
                                <tr>
                                    <td style="padding:28px 30px;background-color:#112240;border-bottom:1px solid #233554;">
                                        <div style="font-size:14px;letter-spacing:1px;text-transform:uppercase;color:#64ffda;font-weight:bold;">SuperBodega</div>
                                        <h1 style="margin:10px 0 6px;color:#e8f4f8;font-size:28px;line-height:1.2;">Gracias por tu compra</h1>
                                        <p style="margin:0;color:#a8b2d1;font-size:15px;">Recibimos tu pedido <strong style="color:#64ffda;">{Html(pedido.NumeroVenta)}</strong>.</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:24px 30px;">
                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:collapse;">
                                            <tr>
                                                <td style="padding:18px;background-color:#0a192f;border:1px solid #233554;border-radius:8px;">
                                                    <div style="color:#64ffda;font-size:16px;font-weight:bold;margin-bottom:10px;">Cliente</div>
                                                    <div style="color:#e8f4f8;font-size:15px;font-weight:bold;">{clienteNombre}</div>
                                                    <div style="color:#a8b2d1;font-size:14px;margin-top:6px;">{Html(pedido.Cliente.Email)}</div>
                                                    <div style="color:#a8b2d1;font-size:14px;">Telefono: {telefono}</div>
                                                    <div style="color:#a8b2d1;font-size:14px;">Direccion: {direccion}</div>
                                                </td>
                                            </tr>
                                        </table>

                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:collapse;margin-top:16px;">
                                            <tr>
                                                <td style="padding:18px;background-color:#0a192f;border:1px solid #233554;border-radius:8px;">
                                                    <div style="color:#64ffda;font-size:16px;font-weight:bold;margin-bottom:10px;">Pedido</div>
                                                    <div style="color:#a8b2d1;font-size:14px;">Fecha: {fecha}</div>
                                                    <div style="color:#e8f4f8;font-size:24px;font-weight:bold;margin-top:8px;">Total: <span style="color:#64ffda;">{FormatMoney(pedido.Total)}</span></div>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:0 30px 28px;">
                                        <h2 style="margin:0 0 12px;color:#64ffda;font-size:18px;">Detalle del pedido</h2>
                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:collapse;border:1px solid #233554;border-radius:8px;overflow:hidden;">
                                            <thead>
                                                <tr style="background-color:#233554;">
                                                    <th style="padding:12px 16px;color:#64ffda;font-size:13px;text-align:left;">Producto</th>
                                                    <th style="padding:12px 16px;color:#64ffda;font-size:13px;text-align:right;">Cant.</th>
                                                    <th style="padding:12px 16px;color:#64ffda;font-size:13px;text-align:right;">Precio</th>
                                                    <th style="padding:12px 16px;color:#64ffda;font-size:13px;text-align:right;">Subtotal</th>
                                                </tr>
                                            </thead>
                                            <tbody>{rows}</tbody>
                                            <tfoot>
                                                <tr style="background-color:#0a192f;">
                                                    <td colspan="3" style="padding:16px;color:#e8f4f8;font-size:16px;text-align:right;"><strong>Total</strong></td>
                                                    <td style="padding:16px;color:#64ffda;font-size:18px;font-weight:bold;text-align:right;">{FormatMoney(pedido.Total)}</td>
                                                </tr>
                                            </tfoot>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:20px 30px;background-color:#0a192f;border-top:1px solid #233554;color:#a8b2d1;font-size:13px;text-align:center;">
                                        Este correo confirma la recepcion de tu pedido. Te avisaremos cuando cambie de estado.
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
            """;
    }

    private static string FormatMoney(decimal value)
    {
        return $"Q{value.ToString("N2", GuatemalaCulture)}";
    }

    private static string Html(string value)
    {
        return HtmlEncoder.Default.Encode(value);
    }
}

file static class PedidoEmailMessageExtensions
{
    public static string ClienteTelefono(this PedidoEmailMessage pedido)
    {
        return string.IsNullOrWhiteSpace(pedido.Cliente.Telefono)
            ? "No registrado"
            : pedido.Cliente.Telefono;
    }
}
