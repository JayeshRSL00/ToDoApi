using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.DTO;
using ToDoApi.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace ToDoApi.Services;

public class ToDoService : IToDoService
{

    private readonly ToDoDbContext _dbContext;
    private readonly IDatabase _redisCache;

    public ToDoService(ToDoDbContext dbContext, IDatabase redisCache)
    {
        _dbContext = dbContext;
        _redisCache = redisCache;
    }

    public async Task<List<Todo>> GetAllToDosAsync()
    {
        return await _dbContext.ToDos.ToListAsync();
        
    }

    public async Task<Todo> CreateTodoAsync(CreateRequest request)
    {
        var todo = new Todo
        {
            Name = request.Name,
            Description = request.Description,
            DueDate = request.DueDate,
            Status = ToDoStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();
        return todo;
    }

    public async Task<Todo?> GetById(int id)
    {
        var cacheKey = $"todo_{id}";
        var cached = await _redisCache.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            Console.WriteLine("Cached response returning");
            return JsonSerializer.Deserialize<Todo>(cached.ToString());
        }

        Todo? todo = await _dbContext.ToDos.FindAsync(id);
        if (todo is not null)
        {
            await _redisCache.StringSetAsync($"todo_{id}", JsonSerializer.Serialize<Todo>(todo).ToString(), TimeSpan.FromMinutes(10));
        }
        return todo;

    }

    public async Task<bool> DeleteById(int id)
    {
        Todo? todo = await _dbContext.ToDos.FindAsync(id);
        if (todo is null)
        {
            return false;
        }
        
        _dbContext.ToDos.Remove(todo);
        await _dbContext.SaveChangesAsync();
        await _redisCache.KeyDeleteAsync($"todo_{id}");
        return true;

    }

    public async Task<Todo?> UpdateToDo(int id, UpdateRequest updateRequest)
    {
        Todo? todo = await _dbContext.ToDos.FindAsync(id);
        if (todo is null)
        {
            return null;
        }
        
        if (updateRequest.Name is not null)
        {
            todo.Name = updateRequest.Name;
        }
        if (updateRequest.DueDate is not null)
        {
            todo.DueDate = updateRequest.DueDate;
        }
        if (updateRequest.Description is not null)
        {
            todo.Description = updateRequest.Description;
        }
        if (updateRequest.Status.HasValue)
        {
            todo.Status = updateRequest.Status.Value;
        }
        
        await _dbContext.SaveChangesAsync();
        await _redisCache.KeyDeleteAsync($"todo_{id}"); // Evict stale values, repopulate on next GET
        return todo;
    }
}