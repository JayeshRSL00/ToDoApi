using System.ComponentModel.DataAnnotations;

namespace ToDoApi.DTO;

public class CreateRequest
{
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(150)]
    public string? Description { get; set; }
    
    public DateTime? DueDate { get; set; }
}