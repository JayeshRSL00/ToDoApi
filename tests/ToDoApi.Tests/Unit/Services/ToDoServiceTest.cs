using Microsoft.EntityFrameworkCore;
using Moq;
using StackExchange.Redis;
using ToDoApi.Data;
using ToDoApi.DTO;
using ToDoApi.Models;
using ToDoApi.Services;
using ToDoApi.UnitTests.Fixtures;
using Xunit;

public class ToDoServiceTest
{
    private readonly ToDoDbContext _dbContext;
    private readonly ToDoService _todoService;

    public ToDoServiceTest()
    {
        _dbContext = new DbContextFixture().CreateDbContext();
        _todoService = new ToDoService(_dbContext, new Mock<IDatabase>().Object);
    }

    [Fact]
    public async Task CreateTodoAsync_Should_CreateTodo_When_RequestIsValid()
    {
        var request = new CreateRequest
        {
            Name = "New Todo",
            Description = "A new todo item",
            DueDate = DateTime.UtcNow.AddDays(2)
        };

        var result = await _todoService.CreateTodoAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.DueDate, result.DueDate);
        Assert.Equal(1, await _dbContext.ToDos.CountAsync());
    }

    [Fact]
    public async Task CreateTodoAsync_Should_SetStatusToPending_When_CreatingTodo()
    {
        var request = new CreateRequest { Name = "Pending Todo" };

        var result = await _todoService.CreateTodoAsync(request);

        Assert.Equal(ToDoStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateTodoAsync_Should_SetCreatedAt_When_CreatingTodo()
    {
        var before = DateTime.UtcNow;
        var request = new CreateRequest { Name = "Timestamped Todo" };

        var result = await _todoService.CreateTodoAsync(request);
        var after = DateTime.UtcNow;

        Assert.InRange(result.CreatedAt, before.AddSeconds(-5), after.AddSeconds(5));
    }

    [Fact]
    public async Task GetAllToDosAsync_Should_ReturnEmptyList_When_NoTodosExist()
    {
        var result = await _todoService.GetAllToDosAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllToDosAsync_Should_ReturnAllTodos_When_TodosExist()
    {
        _dbContext.ToDos.AddRange(
            new Todo { Name = "First", Status = ToDoStatus.Pending },
            new Todo { Name = "Second", Status = ToDoStatus.Completed });
        await _dbContext.SaveChangesAsync();

        var result = await _todoService.GetAllToDosAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, todo => todo.Name == "First");
        Assert.Contains(result, todo => todo.Name == "Second");
    }

    [Fact]
    public async Task GetById_Should_ReturnTodo_When_TodoExistsInCache()
    {
        var todo = new Todo { Id = 1, Name = "Cached Todo", Status = ToDoStatus.Pending };
        var cacheKey = $"todo_{todo.Id}";
        var serializedTodo = System.Text.Json.JsonSerializer.Serialize(todo);

        var redisMock = new Mock<IDatabase>();
        redisMock.Setup(r => r.StringGetAsync(cacheKey, It.IsAny<CommandFlags>()))
            .ReturnsAsync(serializedTodo);

        var serviceWithCache = new ToDoService(_dbContext, redisMock.Object);

        var result = await serviceWithCache.GetById(todo.Id);

        Assert.NotNull(result);
        Assert.Equal(todo.Name, result.Name);
    }

    [Fact]
    public async Task GetById_Should_NotQueryDatabase_When_CacheHit()
    {
        var todo = new Todo { Id = 2, Name = "Cache Hit Todo", Status = ToDoStatus.Pending };
        var cacheKey = $"todo_{todo.Id}";
        var serializedTodo = System.Text.Json.JsonSerializer.Serialize(todo);

        var redisMock = new Mock<IDatabase>();
        redisMock.Setup(r => r.StringGetAsync(cacheKey, It.IsAny<CommandFlags>()))
            .ReturnsAsync(serializedTodo);

        var serviceWithCache = new ToDoService(_dbContext, redisMock.Object);

        var result = await serviceWithCache.GetById(todo.Id);

        Assert.NotNull(result);
        Assert.Equal(todo.Name, result.Name);
        redisMock.Verify(r => r.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task GetById_Should_ReturnTodo_When_TodoExistsInDatabase()
    {
        var todo = new Todo { Name = "Database Todo", Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var redisMock = new Mock<IDatabase>();
        redisMock.Setup(r => r.StringGetAsync($"todo_{todo.Id}", It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var serviceWithCache = new ToDoService(_dbContext, redisMock.Object);

        var result = await serviceWithCache.GetById(todo.Id);

        Assert.NotNull(result);
        Assert.Equal(todo.Name, result.Name);
    }

    [Fact]
    public async Task GetById_Should_StoreTodoInCache_When_CacheMissOccurs()
    {
        var todo = new Todo { Id = 3, Name = "Cache Miss Todo", Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var redisMock = new Mock<IDatabase>();
        redisMock.Setup(r => r.StringGetAsync($"todo_{todo.Id}", It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);
        redisMock.Setup(r => r.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var serviceWithCache = new ToDoService(_dbContext, redisMock.Object);

        var result = await serviceWithCache.GetById(todo.Id);

        Assert.NotNull(result);
        Assert.Contains(redisMock.Invocations, invocation => invocation.Method.Name == nameof(IDatabase.StringSetAsync));
    }

    [Fact]
    public async Task GetById_Should_ReturnNull_When_TodoDoesNotExist()
    {
        var redisMock = new Mock<IDatabase>();
        redisMock.Setup(r => r.StringGetAsync("todo_999", It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var serviceWithCache = new ToDoService(_dbContext, redisMock.Object);

        var result = await serviceWithCache.GetById(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteById_Should_DeleteTodo_When_TodoExists()
    {
        var todo = new Todo { Name = "Delete Me", Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var result = await _todoService.DeleteById(todo.Id);

        Assert.True(result);
        Assert.Empty(await _dbContext.ToDos.ToListAsync());
    }

    [Fact]
    public async Task DeleteById_Should_DeleteCache_When_TodoDeleted()
    {
        var todo = new Todo { Name = "Delete With Cache", Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var redisMock = new Mock<IDatabase>();
        var serviceWithCache = new ToDoService(_dbContext, redisMock.Object);

        await serviceWithCache.DeleteById(todo.Id);

        redisMock.Verify(r => r.KeyDeleteAsync($"todo_{todo.Id}", It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task DeleteById_Should_ReturnTrue_When_DeleteSucceeds()
    {
        var todo = new Todo { Name = "Delete True", Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var result = await _todoService.DeleteById(todo.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteById_Should_ReturnFalse_When_TodoDoesNotExist()
    {
        var result = await _todoService.DeleteById(999);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteById_Should_NotDeleteCache_When_TodoDoesNotExist()
    {
        var redisMock = new Mock<IDatabase>();
        var serviceWithCache = new ToDoService(_dbContext, redisMock.Object);

        var result = await serviceWithCache.DeleteById(999);

        Assert.False(result);
        redisMock.Verify(r => r.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task UpdateToDo_Should_UpdateAllFields_When_AllFieldsProvided()
    {
        var todo = new Todo { Name = "Original", Description = "Old", DueDate = DateTime.UtcNow.AddDays(1), Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateRequest
        {
            Name = "Updated",
            Description = "New description",
            DueDate = DateTime.UtcNow.AddDays(3),
            Status = ToDoStatus.Completed
        };

        var result = await _todoService.UpdateToDo(todo.Id, updateRequest);

        Assert.NotNull(result);
        Assert.Equal(updateRequest.Name, result.Name);
        Assert.Equal(updateRequest.Description, result.Description);
        Assert.Equal(updateRequest.DueDate, result.DueDate);
        Assert.Equal(updateRequest.Status, result.Status);
    }

    [Fact]
    public async Task UpdateToDo_Should_UpdateOnlyProvidedFields_When_RequestIsPartial()
    {
        var todo = new Todo { Name = "Original", Description = "Old", DueDate = DateTime.UtcNow.AddDays(1), Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateRequest { Description = "Only description changed" };

        var result = await _todoService.UpdateToDo(todo.Id, updateRequest);

        Assert.NotNull(result);
        Assert.Equal("Original", result.Name);
        Assert.Equal("Only description changed", result.Description);
        Assert.Equal(todo.DueDate, result.DueDate);
        Assert.Equal(ToDoStatus.Pending, result.Status);
    }

    [Fact]
    public async Task UpdateToDo_Should_NotUpdateName_When_NameIsNull()
    {
        var todo = new Todo { Name = "Original", Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var result = await _todoService.UpdateToDo(todo.Id, new UpdateRequest { Name = null });

        Assert.NotNull(result);
        Assert.Equal("Original", result.Name);
    }

    [Fact]
    public async Task UpdateToDo_Should_NotUpdateDescription_When_DescriptionIsNull()
    {
        var todo = new Todo { Name = "Original", Description = "Existing", Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var result = await _todoService.UpdateToDo(todo.Id, new UpdateRequest { Description = null });

        Assert.NotNull(result);
        Assert.Equal("Existing", result.Description);
    }

    [Fact]
    public async Task UpdateToDo_Should_NotUpdateDueDate_When_DueDateIsNull()
    {
        var todo = new Todo { Name = "Original", DueDate = DateTime.UtcNow.AddDays(1), Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var result = await _todoService.UpdateToDo(todo.Id, new UpdateRequest { DueDate = null });

        Assert.NotNull(result);
        Assert.Equal(todo.DueDate, result.DueDate);
    }

    [Fact]
    public async Task UpdateToDo_Should_NotUpdateStatus_When_StatusIsNull()
    {
        var todo = new Todo { Name = "Original", Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var result = await _todoService.UpdateToDo(todo.Id, new UpdateRequest { Status = null });

        Assert.NotNull(result);
        Assert.Equal(ToDoStatus.Pending, result.Status);
    }

    [Fact]
    public async Task UpdateToDo_Should_DeleteCache_When_TodoUpdated()
    {
        var todo = new Todo { Name = "Original", Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var redisMock = new Mock<IDatabase>();
        var serviceWithCache = new ToDoService(_dbContext, redisMock.Object);

        await serviceWithCache.UpdateToDo(todo.Id, new UpdateRequest { Name = "Updated" });

        redisMock.Verify(r => r.KeyDeleteAsync($"todo_{todo.Id}", It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task UpdateToDo_Should_ReturnUpdatedTodo_When_UpdateSucceeds()
    {
        var todo = new Todo { Name = "Original", Status = ToDoStatus.Pending };
        _dbContext.ToDos.Add(todo);
        await _dbContext.SaveChangesAsync();

        var result = await _todoService.UpdateToDo(todo.Id, new UpdateRequest { Name = "Updated" });

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task UpdateToDo_Should_ReturnNull_When_TodoDoesNotExist()
    {
        var result = await _todoService.UpdateToDo(999, new UpdateRequest { Name = "Updated" });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateToDo_Should_NotDeleteCache_When_TodoDoesNotExist()
    {
        var redisMock = new Mock<IDatabase>();
        var serviceWithCache = new ToDoService(_dbContext, redisMock.Object);

        var result = await serviceWithCache.UpdateToDo(999, new UpdateRequest { Name = "Updated" });

        Assert.Null(result);
        redisMock.Verify(r => r.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Never);
    }
}