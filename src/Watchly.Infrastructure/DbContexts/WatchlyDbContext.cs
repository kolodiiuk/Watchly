using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Watchly.Domain.Entities;
using Watchly.Infrastructure.Extensions;

namespace Watchly.Infrastructure.DbContexts;

public class WatchlyDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public WatchlyDbContext(DbContextOptions<WatchlyDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.UseSnakeCaseNamingConvention();
        builder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(WatchlyDbContext)));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=atark;Username=myuser;Password=mypassword;");
    }
}
