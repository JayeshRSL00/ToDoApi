using Microsoft.EntityFrameworkCore;
using ToDoApi.Models;

namespace ToDoApi.Data;

public class ToDoDbContext : DbContext
{
    public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
    {}
    
    public DbSet<Todo> ToDos { get; set; }
}