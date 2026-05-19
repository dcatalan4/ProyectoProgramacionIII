namespace SuperBodega.Domain.Entities;

public sealed class NotificacionPedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VentaId { get; set; }
    public Venta? Venta { get; set; }
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public TipoNotificacionPedido Tipo { get; set; } = TipoNotificacionPedido.Email;
    public string Destinatario { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool FueEnviada { get; set; }
    public DateTime CreadaUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EnviadaUtc { get; set; }
}
