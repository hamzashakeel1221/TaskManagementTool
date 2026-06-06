using TaskManagement.API.Models;

namespace TaskManagement.API.DTOs;

public record CreateTaskDto(
    string Title,
    string Description,
    TaskPriority Priority,
    int CategoryId,
    DateTime? DueDate,
    string? AssignedToId
);

public record UpdateTaskDto(
    string Title,
    string Description,
    TaskPriority Priority,
    Models.TaskStatus Status,
    int CategoryId,
    DateTime? DueDate,
    string? AssignedToId
);

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public string? AssignedToId { get; set; }
}