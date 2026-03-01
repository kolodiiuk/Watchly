using Testcontainers.PostgreSql;

namespace Watchly.Tests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("watchly")
        .WithUsername("nk")
        .WithPassword("G4thgw4GRETG%$WEgr,dfe45")
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    public Task InitializeAsync()
    {
        return _dbContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _dbContainer.DisposeAsync().AsTask();
    }
}
