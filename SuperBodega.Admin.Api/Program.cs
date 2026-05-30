using SuperBodega.Infrastructure;
using SuperBodega.Admin.Api.Services.Categorias;
using SuperBodega.Admin.Api.Services.Clientes;
using SuperBodega.Admin.Api.Services.Compras;
using SuperBodega.Admin.Api.Services.Productos;
using SuperBodega.Admin.Api.Services.Proveedores;
using SuperBodega.Admin.Api.Services.Reportes;
using SuperBodega.Admin.Api.Services.Ventas;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSuperBodegaInfrastructure(builder.Configuration);
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var productos = scope.ServiceProvider.GetRequiredService<IProductoService>();
    var actualizados = await productos.BackfillIdOriginalesAsync(CancellationToken.None);
    if (actualizados > 0)
    {
        app.Logger.LogInformation(
            "Se recuperaron {Count} IdOriginal de productos existentes.",
            actualizados);
    }
}

app.Run();
