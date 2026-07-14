using System.ComponentModel.DataAnnotations;
using ToDoApi.Models;


namespace ToDoApi.DTO;

public class UpdateRequest
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }

    [MaxLength(150)]
    public string? Description { get; set; }

    public ToDoStatus? Status { get; set; }

    public DateTime? DueDate { get; set; }
}