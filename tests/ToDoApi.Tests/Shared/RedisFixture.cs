using Testcontainers.Redis;

namespace ToDoApi.Tests.Shared;

public class RedisFixture : IAsyncLifetime
{
    private bool _initialized;

    public RedisContainer Container { get; } = new RedisBuilder("redis:latest").Build();

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
