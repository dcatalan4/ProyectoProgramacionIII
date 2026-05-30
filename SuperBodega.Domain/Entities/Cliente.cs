namespace SuperBodega.Domain.Entities;

public sealed class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string IdOriginal { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? DireccionEnvio { get; set; }
    public DateTime FechaRegistroUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Venta> Ventas { get; set; } = [];
    public ICollection<Carrito> Carritos { get; set; } = [];
    public ICollection<NotificacionPedido> NotificacionesPedido { get; set; } = [];
}
