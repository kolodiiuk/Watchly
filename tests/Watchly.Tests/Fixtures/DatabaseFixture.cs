using Testcontainers.PostgreSql;

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
}
