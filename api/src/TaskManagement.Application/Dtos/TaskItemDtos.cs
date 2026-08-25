using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Dtos;

public class TaskItemCreateDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
}

public class TaskItemUpdateDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
}

public class TaskItemDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
