
using StackExchange.Redis;
using ToDoApi.Tests.Shared;

namespace ToDoApi.Tests.BDD.Support;

public sealed class BddFixture : IDisposable
{
    public CustomWebApplicationFactory Factory { get; }

    public HttpClient Client { get; }

    public IDatabase Redis { get; }

    public BddFixture(
        PostgresFixture postgres,
        RedisFixture redis)
    {
        postgres.InitializeAsync().GetAwaiter().GetResult();
        
        redis.InitializeAsync().GetAwaiter().GetResult();

        Factory = new CustomWebApplicationFactory(
            postgres.ConnectionString,
            redis.ConnectionString);

        Client = Factory.CreateClient();

        Redis = ConnectionMultiplexer
            .Connect(redis.ConnectionString)
            .GetDatabase();
    }

    public void Dispose()
    {
        Factory.Dispose();
    }
}