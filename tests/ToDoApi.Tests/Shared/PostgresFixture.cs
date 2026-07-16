using Testcontainers.PostgreSql;

namespace ToDoApi.Tests.Shared;

public class PostgresFixture : IAsyncLifetime
{
    private bool _initialized;

    public PostgreSqlContainer Container { get; } =
        new PostgreSqlBuilder("postgres:18")
            .WithDatabase("integration_db")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await Container.StartAsync();
        _initialized = true;
    }

    public async Task DisposeAsync()
    {
        if (!_initialized)
        {
            return;
        }

        await Container.DisposeAsync();
        _initialized = false;
    }

    public string ConnectionString => Container.GetConnectionString();
}
