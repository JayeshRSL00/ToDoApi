using ToDoApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ToDoApi.UnitTests.Fixtures;

public class DbContextFixture
{
    public ToDoDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ToDoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ToDoDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}