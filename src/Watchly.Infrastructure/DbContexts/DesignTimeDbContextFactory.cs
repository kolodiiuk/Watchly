using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Watchly.Infrastructure.DbContexts;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WatchlyDbContext>
{
    public WatchlyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WatchlyDbContext>();
        var connectionString = DotNetEnv.Env.GetString("ConnectionStrings__DefaultConnection");
        optionsBuilder.UseNpgsql(connectionString);

        return new WatchlyDbContext(optionsBuilder.Options);
    }
}
