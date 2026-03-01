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

    public DbSet<Comment> Comments { get; set; }

    public DbSet<Episode> Episodes { get; set; }

    public DbSet<Genre> Genres { get; set; }

    public DbSet<Keyword> Keywords { get; set; }

    public DbSet<KeywordTitle> KeywordTitles { get; set; }

    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Season> Seasons { get; set; }

    public DbSet<SpokenLanguage> SpokenLanguages { get; set; }

    public DbSet<Title> Titles { get; set; }

    public DbSet<TitleGenre> TitleGenres { get; set; }

    public DbSet<TitleProductionCompany> TitleProductionCompanies { get; set; }

    public DbSet<TitleSpokenLanguage> TitleSpokenLanguages { get; set; }

    public DbSet<Vote> Votes { get; set; }

    public DbSet<WatchList> WatchLists { get; set; }

    public DbSet<WatchListItem> WatchListItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.UseSnakeCaseNamingConvention();
        builder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(WatchlyDbContext))!);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=watchly;Username=nk;Password=G4thgw4GRETG%$WEgr,dfe45");
        }
    }
}
