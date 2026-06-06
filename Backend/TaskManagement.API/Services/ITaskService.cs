using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskResponseDto>> GetTasksAsync(string userId, bool isAdmin);
    Task<TaskResponseDto?> GetTaskByIdAsync(int id, string userId, bool isAdmin);
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, string ownerId);
    Task<TaskResponseDto> UpdateTaskAsync(int id, UpdateTaskDto dto, string userId, bool isAdmin);
    Task DeleteTaskAsync(int id, string userId, bool isAdmin);
}