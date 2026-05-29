namespace SuperBodega.Domain.Entities;

public enum EstadoSolicitud
{
    Encolado,
    Procesando,
    Completado,
    Fallido
}

public sealed class SolicitudPedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarritoId { get; set; }
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Encolado;
    public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcesadoUtc { get; set; }
    public string? MensajeError { get; set; }
    public Guid? VentaId { get; set; }
    public Venta? Venta { get; set; }
}
