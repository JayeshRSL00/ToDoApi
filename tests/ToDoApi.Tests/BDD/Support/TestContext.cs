using System.Net.Http;
using ToDoApi.Models;

namespace ToDoApi.Tests.BDD.Support;

public sealed class TestContext
{
    public HttpResponseMessage? Response { get; set; }

    public Todo? Todo { get; set; }

    public List<Todo>? Todos { get; set; }
}