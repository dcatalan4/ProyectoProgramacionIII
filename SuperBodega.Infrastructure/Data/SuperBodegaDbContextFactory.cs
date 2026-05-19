using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SuperBodega.Infrastructure.Data;

public sealed class SuperBodegaDbContextFactory : IDesignTimeDbContextFactory<SuperBodegaDbContext>
{
    public SuperBodegaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SuperBodegaDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=superbodega;Username=superbodega;Password=superbodega")
            .Options;

        return new SuperBodegaDbContext(options);
    }
}
