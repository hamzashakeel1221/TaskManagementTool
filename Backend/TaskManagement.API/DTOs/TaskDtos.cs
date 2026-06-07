using System.Text.Json.Serialization;

namespace TaskManagement.API.DTOs;

public record CreateTaskDto(
    [property: JsonRequired] string Title,
    [property: JsonRequired] string Description,
    [property: JsonRequired] string Priority,
    [property: JsonRequired] int CategoryId,
    DateTime? DueDate,
    string? AssignedToId
);

public record UpdateTaskDto(
    string? Title,
    string? Description,
    string? Priority,
    [property: JsonRequired] string Status,
    int? CategoryId,
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