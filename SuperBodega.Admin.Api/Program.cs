using SuperBodega.Infrastructure;
using SuperBodega.Admin.Api.Services.Clientes;
using SuperBodega.Admin.Api.Services.Compras;
using SuperBodega.Admin.Api.Services.Productos;
using SuperBodega.Admin.Api.Services.Proveedores;
using SuperBodega.Admin.Api.Services.Reportes;
using SuperBodega.Admin.Api.Services.Ventas;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSuperBodegaInfrastructure(builder.Configuration);
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
