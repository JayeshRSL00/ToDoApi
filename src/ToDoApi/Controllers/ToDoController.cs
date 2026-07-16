using Microsoft.AspNetCore.Mvc;
using ToDoApi.DTO;
using ToDoApi.Models;
using ToDoApi.Services;

namespace ToDoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToDoController : ControllerBase
{
    private readonly IToDoService _todoService;

    public ToDoController(IToDoService toDoService)
    {
        _todoService = toDoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Todo>>> GetAllToDos()
    {
        var todos = await _todoService.GetAllToDosAsync();
        return Ok(todos);
    }

    [HttpPost]
    public async Task<ActionResult<Todo>> CreateToDo([FromBody] CreateRequest createBody)
    {
        var todo = await _todoService.CreateTodoAsync(createBody);
        return Ok(todo);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Todo?>> GetToDoById(int id)
    {
        var todo = await _todoService.GetById(id);
        if (todo is null)
        {
            return NotFound();
        }
        return Ok(todo);
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteToDoById(int id)
    {
        bool isDeleted = await _todoService.DeleteById(id);
        if (isDeleted)
        {
            return NoContent();
        }
        return NotFound();
    }

    [HttpDelete]
    public async Task<ActionResult<bool>> DeleteAllToDos()
    {
        bool isDeleted = await _todoService.DeleteAll();
        if (isDeleted)
        {
            return NoContent();
        }
        return NotFound();
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<Todo>> UpdateToDo(int id, [FromBody] UpdateRequest updateRequest)
    {
        Todo? todo = await _todoService.UpdateToDo(id, updateRequest);
        if (todo is null)
        {
            return NotFound();
        }
        return Ok(todo);
    }
}

