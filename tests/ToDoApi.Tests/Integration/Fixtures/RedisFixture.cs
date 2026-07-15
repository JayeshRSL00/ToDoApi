using Testcontainers.Redis;

namespace ToDoApi.Tests.Integration.Fixtures;

public class RedisFixture : IAsyncLifetime
{
    public RedisContainer Container { get; } = new RedisBuilder("redis:latest").Build();

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