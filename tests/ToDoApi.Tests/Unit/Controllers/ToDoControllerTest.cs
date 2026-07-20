using Microsoft.AspNetCore.Mvc;
using Moq;
using ToDoApi.Controllers;
using ToDoApi.Models;
using ToDoApi.DTO;

namespace ToDoApi.Tests.Unit.Controllers
{
    public class ToDoControllerTest
    {
        private readonly Mock<IToDoService> _mockService;
        private readonly ToDoController _controller;

        public ToDoControllerTest()
        {
            _mockService = new Mock<IToDoService>();
            _controller = new ToDoController(_mockService.Object);
        }

        [Fact]
        public async Task GetAllToDos_ReturnsOkWithList()
        {
            var todos = new List<Todo> { new Todo { Id = 1, Name = "t" } };
            _mockService.Setup(s => s.GetAllToDosAsync()).ReturnsAsync(todos);

            var result = await _controller.GetAllToDos();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(todos, ok.Value);
        }

        [Fact]
        public async Task GetToDoById_ReturnsOkWhenFound()
        {
            var todo = new Todo { Id = 1, Name = "t" };
            _mockService.Setup(s => s.GetById(1)).ReturnsAsync(todo);

            var result = await _controller.GetToDoById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(todo, ok.Value);
        }

        [Fact]
        public async Task GetToDoById_ReturnsNotFoundWhenMissing()
        {
            _mockService.Setup(s => s.GetById(2)).ReturnsAsync((Todo?)null);

            var result = await _controller.GetToDoById(2);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateToDo_ReturnsOkWithCreated()
        {
            var req = new CreateRequest { Name = "new" };
            var created = new Todo { Id = 5, Name = "new" };
            _mockService.Setup(s => s.CreateTodoAsync(req)).ReturnsAsync(created);

            var result = await _controller.CreateToDo(req);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(created, ok.Value);
        }

        [Fact]
        public async Task DeleteToDoById_ReturnsNoContentWhenDeleted()
        {
            _mockService.Setup(s => s.DeleteById(1)).ReturnsAsync(true);

            var result = await _controller.DeleteToDoById(1);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public async Task DeleteToDoById_ReturnsNotFoundWhenMissing()
        {
            _mockService.Setup(s => s.DeleteById(2)).ReturnsAsync(false);

            var result = await _controller.DeleteToDoById(2);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateToDo_ReturnsOkWhenUpdated()
        {
            var req = new UpdateRequest { Name = "u" };
            var updated = new Todo { Id = 3, Name = "u" };
            _mockService.Setup(s => s.UpdateToDo(3, req)).ReturnsAsync(updated);

            var result = await _controller.UpdateToDo(3, req);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(updated, ok.Value);
        }

        [Fact]
        public async Task UpdateToDo_ReturnsNotFoundWhenMissing()
        {
            var req = new UpdateRequest { Name = "u" };
            _mockService.Setup(s => s.UpdateToDo(4, req)).ReturnsAsync((Todo?)null);

            var result = await _controller.UpdateToDo(4, req);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
