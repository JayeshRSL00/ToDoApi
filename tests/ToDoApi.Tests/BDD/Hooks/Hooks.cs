using Reqnroll;
using ToDoApi.Tests.BDD.Support;

namespace ToDoApi.Tests.BDD.Hooks;


[Binding]
public sealed class Hooks
{
    [BeforeScenario]
    public async Task BeforeScenario(
        BddFixture fixture)
    {
        
    }
}