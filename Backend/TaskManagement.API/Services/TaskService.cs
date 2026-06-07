using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;

namespace TaskManagement.API.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(AppDbContext context, ILogger<TaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetTasksAsync(string userId, bool isAdmin)
    {
        var query = _context.Tasks
            .Include(t => t.Category)
            .Include(t => t.Owner)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (!isAdmin)
            query = query.Where(t => t.OwnerId == userId || t.AssignedToId == userId);

        return await query.Select(t => MapToDto(t)).ToListAsync();
    }

    public async Task<TaskResponseDto?> GetTaskByIdAsync(int id, string userId, bool isAdmin)
    {
        var task = await _context.Tasks
            .Include(t => t.Category)
            .Include(t => t.Owner)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return null;

        if (!isAdmin && task.OwnerId != userId && task.AssignedToId != userId)
            throw new UnauthorizedAccessException("You do not have permission to view this task.");

        return MapToDto(task);
    }

    public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, string ownerId, bool isAdmin)
    {
        // Validate category exists without storing unused variable
        if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            throw new KeyNotFoundException($"Category {dto.CategoryId} not found.");

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = Enum.Parse<TaskPriority>(dto.Priority, true),
            CategoryId = dto.CategoryId,
            DueDate = dto.DueDate,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AssignedToId = isAdmin ? dto.AssignedToId : null
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        await _context.Entry(task).Reference(t => t.Category).LoadAsync();
        await _context.Entry(task).Reference(t => t.Owner).LoadAsync();
        if (task.AssignedToId != null)
            await _context.Entry(task).Reference(t => t.AssignedTo).LoadAsync();

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Task '{Title}' created by user {UserId}", task.Title, ownerId);

        return MapToDto(task);
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(int id, UpdateTaskDto dto, string userId, bool isAdmin)
    {
        var task = await _context.Tasks
            .Include(t => t.Category)
            .Include(t => t.Owner)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Task {id} not found.");

        bool isOwner = task.OwnerId == userId;
        bool isAssigned = task.AssignedToId == userId;

        if (!isOwner && !isAssigned)
            throw new UnauthorizedAccessException("You do not have permission to edit this task.");

        if (isOwner)
        {
            if (dto.Title != null) task.Title = dto.Title;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.Priority != null) task.Priority = Enum.Parse<TaskPriority>(dto.Priority, true);
            if (dto.CategoryId != null) task.CategoryId = dto.CategoryId.Value;
            task.DueDate = dto.DueDate;
            task.AssignedToId = dto.AssignedToId;
        }

        task.Status = Enum.Parse<Models.TaskStatus>(dto.Status, true);
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Task {TaskId} updated by user {UserId}", id, userId);

        return MapToDto(task);
    }

    public async Task DeleteTaskAsync(int id, string userId)
    {
        var task = await _context.Tasks.FindAsync(id)
            ?? throw new KeyNotFoundException($"Task {id} not found.");

        if (task.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the task owner can delete this task.");

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Task {TaskId} deleted by user {UserId}", id, userId);
    }

    private static TaskResponseDto MapToDto(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status.ToString(),
        Priority = t.Priority.ToString(),
        DueDate = t.DueDate,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        CategoryName = t.Category?.Name ?? "",
        CategoryId = t.CategoryId,
        OwnerName = t.Owner?.FullName ?? "",
        OwnerId = t.OwnerId,
        AssignedToName = t.AssignedTo?.FullName,
        AssignedToId = t.AssignedToId
    };
}