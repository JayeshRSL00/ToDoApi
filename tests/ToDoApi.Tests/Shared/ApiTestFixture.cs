using System.Net.Http;

namespace ToDoApi.Tests.Shared;

public class ApiTestFixture : IDisposable
{
    public HttpClient Client { get; }

    public ApiTestFixture()
    {
        Client = new HttpClient { BaseAddress = new Uri("http://localhost") };
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}
