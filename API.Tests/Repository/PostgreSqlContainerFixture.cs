using Testcontainers.PostgreSql;
using Xunit;

namespace API.Tests.Repository;

public class PostgreSQLContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    
    public string ConnectionString { get; private set; } = string.Empty;

    public PostgreSQLContainerFixture()
    {
         _container = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("careerhub_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}