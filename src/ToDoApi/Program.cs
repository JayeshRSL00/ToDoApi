using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using ToDoApi.Data;
using ToDoApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<ToDoService>();
builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres")
    );
});
builder.Services.AddSingleton<StackExchange.Redis.IDatabase>(sp =>
{
    var muxer = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
    return muxer.GetDatabase();
});

var app = builder.Build();

// Automatically apply pending EF Core migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "My API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
