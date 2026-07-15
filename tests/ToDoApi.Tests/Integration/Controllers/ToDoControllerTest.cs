using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using StackExchange.Redis;
using ToDoApi.Tests.Integration.Fixtures;
using ToDoApi.Tests.Integration.Infrastructure;
using Xunit;
using ToDoApi.Models;

namespace ToDoApi.Tests.Integration.Controllers
{
    [Collection("IntegrationCollection")]
    public sealed class ToDoControllerTest : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabase _redisDatabase;

        public ToDoControllerTest(PostgresFixture postgresFixture, RedisFixture redisFixture)
        {
            _factory = new CustomWebApplicationFactory(
                postgresFixture.ConnectionString,
                redisFixture.ConnectionString);

            _redisConnection = ConnectionMultiplexer.Connect(redisFixture.ConnectionString);
            _redisDatabase = _redisConnection.GetDatabase();
        }

        public void Dispose()
        {
            _redisConnection.Dispose();
            _factory.Dispose();
        }

        [Fact]
        public async Task GetTodos_ReturnsSuccessStatusCode()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/todo");

            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task CreateTodo_ReturnsCreatedTodo()
        {
            var client = _factory.CreateClient();
            var newTodo = new { Name = "Integration test todo", Status = ToDoStatus.Pending };

            var response = await client.PostAsJsonAsync("/api/todo", newTodo);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var createdTodo = await response.Content.ReadFromJsonAsync<Todo>();
            Assert.NotNull(createdTodo);
            Assert.Equal(newTodo.Name, createdTodo?.Name);
            Assert.Equal(newTodo.Status, createdTodo?.Status);
            Assert.True(createdTodo?.Id > 0);
        }

        [Fact]
        public async Task GetTodo_CachesResultInRedis()
        {
            var client = _factory.CreateClient();
            var createResponse = await client.PostAsJsonAsync("/api/todo", new { Name = "Cache test todo", Status = ToDoStatus.Pending });
            createResponse.EnsureSuccessStatusCode();

            var createdTodo = await createResponse.Content.ReadFromJsonAsync<Todo>();
            Assert.NotNull(createdTodo);

            var getResponse = await client.GetAsync($"/api/todo/{createdTodo.Id}");
            getResponse.EnsureSuccessStatusCode();

            var cached = await _redisDatabase.StringGetAsync($"todo_{createdTodo.Id}");
            Assert.True(cached.HasValue, "Expected Redis cache entry to exist after GET.");
            Assert.Contains(createdTodo.Name, cached.ToString());
        }

        [Fact]
        public async Task DeleteTodo_RemovesRedisCacheEntry()
        {
            var client = _factory.CreateClient();
            var createResponse = await client.PostAsJsonAsync("/api/todo", new { Name = "Cache delete todo", Status = ToDoStatus.Pending });
            createResponse.EnsureSuccessStatusCode();

            var createdTodo = await createResponse.Content.ReadFromJsonAsync<Todo>();
            Assert.NotNull(createdTodo);

            var getResponse = await client.GetAsync($"/api/todo/{createdTodo.Id}");
            getResponse.EnsureSuccessStatusCode();

            var cached = await _redisDatabase.StringGetAsync($"todo_{createdTodo.Id}");
            Assert.True(cached.HasValue, "Expected cache entry to exist before delete.");

            var deleteResponse = await client.DeleteAsync($"/api/todo/{createdTodo.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var cachedAfterDelete = await _redisDatabase.StringGetAsync($"todo_{createdTodo.Id}");
            Assert.False(cachedAfterDelete.HasValue, "Expected Redis cache entry to be removed after delete.");
        }

        [Fact]
        public async Task UpdateTodo_ReturnsNoContent()
        {
            var client = _factory.CreateClient();
            var createResponse = await client.PostAsJsonAsync("/api/todo", new { Name = "Todo to update", Status = ToDoStatus.Pending });
            createResponse.EnsureSuccessStatusCode();

            var createdTodo = await createResponse.Content.ReadFromJsonAsync<Todo>();
            Assert.NotNull(createdTodo);

            var updatedTodo = new { id = createdTodo.Id, Name = "Updated todo title", Status = ToDoStatus.Completed };
            var updateResponse = await client.PutAsJsonAsync($"/api/todo/{createdTodo.Id}", updatedTodo);

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var getResponse = await client.GetAsync($"/api/todo/{createdTodo.Id}");
            getResponse.EnsureSuccessStatusCode();

            var fetchedTodo = await getResponse.Content.ReadFromJsonAsync<Todo>();
            Assert.NotNull(fetchedTodo);
            Assert.Equal(updatedTodo.Name, fetchedTodo?.Name);
            Assert.Equal(ToDoStatus.Completed, fetchedTodo?.Status);
        }

        [Fact]
        public async Task DeleteTodo_ReturnsNoContent()
        {
            var client = _factory.CreateClient();
            var createResponse = await client.PostAsJsonAsync("/api/todo", new { Name = "Todo to delete", Status = ToDoStatus.Pending });
            createResponse.EnsureSuccessStatusCode();

            var createdTodo = await createResponse.Content.ReadFromJsonAsync<Todo>();
            Assert.NotNull(createdTodo);

            var deleteResponse = await client.DeleteAsync($"/api/todo/{createdTodo.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await client.GetAsync($"/api/todo/{createdTodo.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}