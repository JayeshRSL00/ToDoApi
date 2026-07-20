namespace ToDoApi.Models;

public class Todo()
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public ToDoStatus Status { get; set; }

}

public enum ToDoStatus
{
    Pending,
    InProgress,
    Completed,
    Archived
}