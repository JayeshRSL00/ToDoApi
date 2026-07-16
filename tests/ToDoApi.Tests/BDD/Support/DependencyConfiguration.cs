using Autofac;
using Reqnroll;
using Reqnroll.Autofac;
using ToDoApi.Tests.BDD.Drivers;
using ToDoApi.Tests.BDD.Hooks;
using ToDoApi.Tests.Shared;

namespace ToDoApi.Tests.BDD.Support;

public static class DependencyConfiguration
{
    [ScenarioDependencies]
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterType<TestContext>()
            .InstancePerLifetimeScope();

        builder.RegisterType<BddFixture>()
            .InstancePerLifetimeScope();

        builder.RegisterType<PostgresFixture>()
            .InstancePerLifetimeScope();

        builder.RegisterType<RedisFixture>()
            .InstancePerLifetimeScope();

        builder.RegisterType<ToDoApiDriver>()
            .InstancePerLifetimeScope();

        builder.RegisterType<ToDoApi.Tests.BDD.Hooks.Hooks>()
            .InstancePerLifetimeScope();

        builder.RegisterType<ToDoApi.Tests.BDD.Steps.ToDoSteps>()
            .InstancePerLifetimeScope();

        builder.RegisterType<ApiTestFixture>()
            .SingleInstance();

        builder.Register(c => c.Resolve<BddFixture>().Client)
            .As<HttpClient>()
            .SingleInstance();
    }
}