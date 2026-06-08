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

        var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return tasks.Select(MapToDto);
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
            throw new UnauthorizedAccessException("You do not have access to this task.");

        return MapToDto(task);
    }

    public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, string ownerId, bool isAdmin)
    {
       
        string? assignedToId = isAdmin ? dto.AssignedToId : null;

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = Enum.Parse<TaskPriority>(dto.Priority),
            CategoryId = dto.CategoryId,
            DueDate = dto.DueDate,
            OwnerId = ownerId,
            AssignedToId = assignedToId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

      
        _logger.LogInformation("Task created: {Title} by user {UserId}", dto.Title, ownerId);

        
        return await GetTaskByIdAsync(task.Id, ownerId, true)
            ?? throw new InvalidOperationException("Failed to retrieve created task.");
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(int id, UpdateTaskDto dto, string userId, bool isAdmin)
    {
        var task = await _context.Tasks.FindAsync(id)
            ?? throw new KeyNotFoundException($"Task {id} not found.");

        bool isOwner = task.OwnerId == userId;
        bool isAssigned = task.AssignedToId == userId;

       
        if (isAdmin && !isOwner)
            throw new UnauthorizedAccessException("Admin can only edit tasks they own.");

        if (!isAdmin && !isOwner && !isAssigned)
            throw new UnauthorizedAccessException("You do not have access to this task.");

        if (isOwner)
        {
           
            if (dto.Title != null) task.Title = dto.Title;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.Priority != null) task.Priority = Enum.Parse<TaskPriority>(dto.Priority);
            if (dto.CategoryId != null) task.CategoryId = dto.CategoryId.Value;
            task.DueDate = dto.DueDate;
          
            task.AssignedToId = isAdmin ? dto.AssignedToId : null;
        }

      
        task.Status = Enum.Parse<Models.TaskStatus>(dto.Status);
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

      
        _logger.LogInformation("Task {Id} updated by user {UserId}", id, userId);

      
        return await GetTaskByIdAsync(id, userId, true)
            ?? throw new InvalidOperationException("Failed to retrieve updated task.");
    }

    public async Task DeleteTaskAsync(int id, string userId)
    {
        var task = await _context.Tasks.FindAsync(id)
            ?? throw new KeyNotFoundException($"Task {id} not found.");

       
        if (task.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the task owner can delete this task.");

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        
        _logger.LogInformation("Task {Id} deleted by user {UserId}", id, userId);
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