
using SuperBodega.Ecommerce.Api.Consumers;
using SuperBodega.Ecommerce.Api.Messaging;
using SuperBodega.Ecommerce.Api.Queues;
using SuperBodega.Ecommerce.Api.Services.Carrito;
using SuperBodega.Ecommerce.Api.Services.Catalogo;
using SuperBodega.Ecommerce.Api.Services.Pedidos;
using SuperBodega.Ecommerce.Api.Workers;
using SuperBodega.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSuperBodegaInfrastructure(builder.Configuration);
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddHostedService<KafkaConsumerService>();

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
