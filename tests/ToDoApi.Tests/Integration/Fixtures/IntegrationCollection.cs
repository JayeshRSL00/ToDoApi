using ToDoApi.Tests.Shared;

namespace ToDoApi.Tests.Integration.Fixtures;

[CollectionDefinition("IntegrationCollection")]
public class IntegrationCollection : ICollectionFixture<PostgresFixture>, ICollectionFixture<RedisFixture> { }