using FluentAssertions;
using Reqnroll;
using ToDoApi.Models;
using ToDoApi.Tests.BDD.Drivers;
using ToDoApi.Tests.BDD.Support;

namespace ToDoApi.Tests.BDD.Steps;

[Binding]
public class ToDoSteps
{
    private readonly ToDoApiDriver _driver;
    private readonly TestContext _testContext;

    public ToDoSteps(ToDoApiDriver driver, TestContext testContext)
    {
        _driver = driver;
        _testContext = testContext;
    }

    [Given(@"the todo list is empty")]
    public async Task TheTodoListIsEmpty()
    {
        await _driver.ClearTodosAsync();
    }

    [Given(@"a todo named ""(.*)"" exists")]
    public async Task GivenATodoNamedExists(string name)
    {
        var createdTodo = await _driver.CreateTodoAsync(name, null);
        _testContext.Todo = createdTodo;
    }

    [When(@"I retrieve all todos")]
    public async Task WhenIRetrieveAllTodos()
    {
        var todos = await _driver.GetAllTodosAsync();
        _testContext.Todos = todos;
    }

    [When(@"I retrieve that todo by id")]
    public async Task WhenIRetrieveThatTodoById()
    {
        var existingTodo = _testContext.Todo;
        var fetchedTodo = await _driver.GetTodoByIdAsync(existingTodo!.Id);
        _testContext.Todo = fetchedTodo;
    }

    [When(@"I update that todo with name ""(.*)"" and status ""(.*)""")]
    public async Task WhenIUpdateThatTodoWithNameAndStatus(string name, ToDoStatus status)
    {
        var existingTodo = _testContext.Todo;
        await _driver.UpdateTodoAsync(existingTodo!.Id, name, status);
        var updatedTodo = await _driver.GetTodoByIdAsync(existingTodo.Id);
        _testContext.Todo = updatedTodo;
    }

    [When(@"I update its name to ""(.*)""")]
    public async Task WhenIUpdateItsNameTo(string name)
    {
        var existingTodo = _testContext.Todo;
        await _driver.UpdateTodoAsync(existingTodo!.Id, name, existingTodo.Status);
        var updatedTodo = await _driver.GetTodoByIdAsync(existingTodo.Id);
        _testContext.Todo = updatedTodo;
    }

    [When(@"I create a todo named ""(.*)"" with description ""(.*)""")]
    public async Task WhenICreateANewTodoWithNameAndDescription(string name, string? description)
    {
        var createdTodo = await _driver.CreateTodoAsync(name, description);
        _testContext.Todo = createdTodo;
    }

    [When(@"I delete that todo")]
    public async Task WhenIDeleteThatTodo()
    {
        var existingTodo = _testContext.Todo;
        await _driver.DeleteTodoAsync(existingTodo!.Id);
        _testContext.Todo = null;
    }

    [Then(@"the returned todo should have the name ""(.*)""")]
    public async Task ThenTheReturnedTodoShouldHaveTheName(string name)
    {
        var returnedTodo = _testContext.Todo;
        returnedTodo.Should().NotBeNull();
        returnedTodo!.Name.Should().Be(name);
    }

    [Then(@"(.*) todo should be returned")]
    public async Task ThenATodoShouldBeReturned(string count)
    {
        var todos = _testContext.Todos;
        todos.Should().NotBeNull();

        var expectedCount = count.Trim('"');
        var actualCount = todos!.Count;

        if (int.TryParse(expectedCount, out var numericCount))
        {
            actualCount.Should().Be(numericCount);
            return;
        }

        switch (expectedCount.ToLowerInvariant())
        {
            case "one":
                actualCount.Should().Be(1);
                break;
            default:
                throw new InvalidOperationException($"Unsupported todo count '{expectedCount}'.");
        }
    }

    [Then(@"the created todo should have name ""(.*)""")]
    public async Task ThenTheCreatedTodoShouldHaveName(string name)
    {
        var createdTodo = _testContext.Todo;
        createdTodo.Should().NotBeNull();
        createdTodo!.Name.Should().Be(name);
    }

    [Then(@"the created todo should have description ""(.*)""")]
    public async Task ThenTheCreatedTodoShouldHaveDescription(string description)
    {
        var createdTodo = _testContext.Todo;
        createdTodo.Should().NotBeNull();
        createdTodo!.Description.Should().Be(description);
    }

    [Then(@"the created todo status should be ""(.*)""")]
    public async Task ThenTheCreatedTodoStatusShouldBe(ToDoStatus status)
    {
        var createdTodo = _testContext.Todo;
        createdTodo.Should().NotBeNull();
        createdTodo!.Status.Should().Be(status);
    }

    [Then(@"the updated todo name should be ""(.*)""")]
    public async Task ThenTheUpdatedTodoNameShouldBe(string name)
    {
        var updatedTodo = _testContext.Todo;
        updatedTodo.Should().NotBeNull();
        updatedTodo!.Name.Should().Be(name);
    }

    [Then(@"the todo should no longer exist")]
    public async Task ThenTheTodoShouldNoLongerExist()
    {
        var deletedTodo = _testContext.Todo;
        deletedTodo.Should().BeNull();
    }
}
