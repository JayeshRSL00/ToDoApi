
using System.Net;
using System.Net.Http.Json;
using ToDoApi.DTO;
using ToDoApi.Models;
using ToDoApi.Tests.BDD.Support;

namespace ToDoApi.Tests.BDD.Drivers;

public class ToDoApiDriver
{
    private readonly HttpClient _client;

    public ToDoApiDriver(HttpClient client)
    {
        _client = client;
    }

    public async Task<Todo> CreateTodoAsync(string name, string? description)
    {
        CreateRequest request = new CreateRequest
        {
            Name = name,
    
        };
        if (!string.IsNullOrEmpty(description))
        {
            request.Description = description;
        }
        var response = await _client.PostAsJsonAsync("/api/todo", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Todo>() ?? throw new InvalidOperationException("Failed to deserialize the created todo.");

    }

    public async Task<Todo> GetTodoByIdAsync(int id)
    {
        var response = await _client.GetAsync($"/api/todo/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Todo>() ?? throw new InvalidOperationException("Failed to deserialize the retrieved todo.");
    }

    public async Task UpdateTodoAsync(int id, string name, ToDoStatus status)
    {
        var response = await _client.PutAsJsonAsync($"/api/todo/{id}", new UpdateRequest
        {
            Name = name,
            Status = status
        });
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTodoAsync(int id)
    {
        var response = await _client.DeleteAsync($"/api/todo/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Todo>> GetAllTodosAsync()
    {
        var response = await _client.GetAsync("/api/todo");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Todo>>() ?? throw new InvalidOperationException("Failed to deserialize the list of todos.");
    }

    public async Task ClearTodosAsync()
    {
        var response = await _client.DeleteAsync("/api/todo");
        if (response.StatusCode != HttpStatusCode.NoContent && response.StatusCode != HttpStatusCode.NotFound)
        {
            response.EnsureSuccessStatusCode();
        }
    }
}