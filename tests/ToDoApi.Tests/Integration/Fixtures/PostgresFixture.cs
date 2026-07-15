using Testcontainers.PostgreSql;

namespace ToDoApi.Tests.Integration.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } =
        new PostgreSqlBuilder("postgres:18")
            .WithDatabase("integration_db")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }

    public string ConnectionString => Container.GetConnectionString();
}