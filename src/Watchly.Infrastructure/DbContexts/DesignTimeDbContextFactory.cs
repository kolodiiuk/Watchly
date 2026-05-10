using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Watchly.Infrastructure.DbContexts;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WatchlyDbContext>
{
    public WatchlyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WatchlyDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=watchly;Username=nk;Password=G4thgw4GRETG%WEgrdfe45");

        return new WatchlyDbContext(optionsBuilder.Options);
    }
}
