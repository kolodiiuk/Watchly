using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Tests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _dbContainer;
    private string? _connectionString;

    public string ConnectionString => _connectionString ?? throw new InvalidOperationException("Container not started yet");

    public async Task InitializeAsync()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("watchly")
            .WithUsername("nk")
            .WithPassword("G4thgw4GRETG%$WEgr,dfe45")
            .Build();

        await _dbContainer.StartAsync();
        _connectionString = _dbContainer.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        if (_dbContainer != null)
        {
            await _dbContainer.DisposeAsync();
        }
    }

    public static Task ResetDatabaseAsync(WatchlyDbContext dbContext)
    {
        const string sql = """
                           TRUNCATE TABLE
                               user_content_activities,
                               user_title_progresses,
                               comments,
                               votes,
                               watch_list_items,
                               watch_lists,
                               refresh_tokens,
                               password_reset_tokens,
                               episodes,
                               seasons,
                               titles,
                               asp_net_users
                           RESTART IDENTITY CASCADE;
                           """;

        return dbContext.Database.ExecuteSqlRawAsync(sql);
    }
}
