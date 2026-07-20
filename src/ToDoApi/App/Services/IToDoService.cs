using ToDoApi.DTO;
using ToDoApi.Models;

public interface IToDoService
{
    Task<List<Todo>> GetAllToDosAsync();
    Task<Todo> CreateTodoAsync(CreateRequest request);
    Task<Todo?> GetById(int id);
    Task<bool> DeleteById(int id);
    Task<Todo?> UpdateToDo(int id, UpdateRequest request);
    Task<bool> DeleteAll();
}