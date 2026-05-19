using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperBodega.Infrastructure.Data;

namespace SuperBodega.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSuperBodegaInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SuperBodegaDb")
            ?? "Host=localhost;Port=5432;Database=superbodega;Username=superbodega;Password=superbodega";

        services.AddDbContext<SuperBodegaDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
