using SuperBodega.Infrastructure;
using SuperBodega.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSuperBodegaInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
