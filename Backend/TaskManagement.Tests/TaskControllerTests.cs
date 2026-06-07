using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using TaskManagement.API.Controllers;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services;

namespace TaskManagement.Tests;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _controller = new TasksController(_taskServiceMock.Object);
        SetupUser("user1", isAdmin: false);
    }

    private void SetupUser(string userId, bool isAdmin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, "test@test.com"),
        };
        if (isAdmin) claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ─── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkWithTasks()
    {
        var tasks = new List<TaskResponseDto>
        {
            new() { Id = 1, Title = "Task 1", OwnerId = "user1" },
            new() { Id = 2, Title = "Task 2", OwnerId = "user1" }
        };
        _taskServiceMock.Setup(x => x.GetTasksAsync("user1", false)).ReturnsAsync(tasks);

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().BeEquivalentTo(tasks);
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsAllTasks()
    {
        SetupUser("admin1", isAdmin: true);
        var tasks = new List<TaskResponseDto>
        {
            new() { Id = 1, Title = "Task 1", OwnerId = "user1" },
            new() { Id = 2, Title = "Task 2", OwnerId = "user2" }
        };
        _taskServiceMock.Setup(x => x.GetTasksAsync("admin1", true)).ReturnsAsync(tasks);

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        (ok!.Value as IEnumerable<TaskResponseDto>)!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoTasks()
    {
        _taskServiceMock.Setup(x => x.GetTasksAsync("user1", false))
            .ReturnsAsync(new List<TaskResponseDto>());

        var result = await _controller.GetAll() as OkObjectResult;

        (result!.Value as IEnumerable<TaskResponseDto>)!.Should().BeEmpty();
    }

    // ─── GetById ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_WithValidId_ReturnsOk()
    {
        var task = new TaskResponseDto { Id = 1, Title = "Task 1", OwnerId = "user1" };
        _taskServiceMock.Setup(x => x.GetTaskByIdAsync(1, "user1", false)).ReturnsAsync(task);

        var result = await _controller.GetById(1);

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().Be(task);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        _taskServiceMock.Setup(x => x.GetTaskByIdAsync(999, "user1", false))
            .ReturnsAsync((TaskResponseDto?)null);

        var result = await _controller.GetById(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_WhenUnauthorized_ThrowsUnauthorizedAccessException()
    {
        _taskServiceMock.Setup(x => x.GetTaskByIdAsync(1, "user1", false))
            .ThrowsAsync(new UnauthorizedAccessException("You do not have access to this task."));

        var act = () => _controller.GetById(1);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ─── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var dto = new CreateTaskDto("New Task", "Description", "High", 1, null, null);
        var task = new TaskResponseDto { Id = 1, Title = "New Task", OwnerId = "user1" };
        _taskServiceMock.Setup(x => x.CreateTaskAsync(dto, "user1", false)).ReturnsAsync(task);

        var result = await _controller.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
        var created = result as CreatedAtActionResult;
        created!.Value.Should().Be(task);
    }

    [Fact]
    public async Task Create_AsAdmin_ReturnsCreated()
    {
        SetupUser("admin1", isAdmin: true);
        var dto = new CreateTaskDto("Admin Task", "Description", "High", 1, null, "user1");
        var task = new TaskResponseDto { Id = 2, Title = "Admin Task", OwnerId = "admin1", AssignedToId = "user1" };
        _taskServiceMock.Setup(x => x.CreateTaskAsync(dto, "admin1", true)).ReturnsAsync(task);

        var result = await _controller.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_WithInvalidCategory_ThrowsKeyNotFoundException()
    {
        var dto = new CreateTaskDto("Task", "Desc", "High", 999, null, null);
        _taskServiceMock.Setup(x => x.CreateTaskAsync(dto, "user1", false))
            .ThrowsAsync(new KeyNotFoundException("Category 999 not found."));

        var act = () => _controller.Create(dto);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_WithValidData_ReturnsOk()
    {
        var dto = new UpdateTaskDto("Updated Title", "Updated Desc", "Low", "Completed", 1, null, null);
        var task = new TaskResponseDto { Id = 1, Title = "Updated Title", OwnerId = "user1" };
        _taskServiceMock.Setup(x => x.UpdateTaskAsync(1, dto, "user1", false)).ReturnsAsync(task);

        var result = await _controller.Update(1, dto);

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().Be(task);
    }

    [Fact]
    public async Task Update_WhenNotOwner_ThrowsUnauthorizedAccessException()
    {
        var dto = new UpdateTaskDto(null, null, null, "Pending", null, null, null);
        _taskServiceMock.Setup(x => x.UpdateTaskAsync(1, dto, "user1", false))
            .ThrowsAsync(new UnauthorizedAccessException("You do not have access to this task."));

        var act = () => _controller.Update(1, dto);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Update_WithInvalidId_ThrowsKeyNotFoundException()
    {
        var dto = new UpdateTaskDto(null, null, null, "Pending", null, null, null);
        _taskServiceMock.Setup(x => x.UpdateTaskAsync(999, dto, "user1", false))
            .ThrowsAsync(new KeyNotFoundException("Task 999 not found."));

        var act = () => _controller.Update(999, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ─── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        _taskServiceMock.Setup(x => x.DeleteTaskAsync(1, "user1")).Returns(Task.CompletedTask);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenNotOwner_ThrowsUnauthorizedAccessException()
    {
        _taskServiceMock.Setup(x => x.DeleteTaskAsync(1, "user1"))
            .ThrowsAsync(new UnauthorizedAccessException("Only the task owner can delete this task."));

        var act = () => _controller.Delete(1);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ThrowsKeyNotFoundException()
    {
        _taskServiceMock.Setup(x => x.DeleteTaskAsync(999, "user1"))
            .ThrowsAsync(new KeyNotFoundException("Task 999 not found."));

        var act = () => _controller.Delete(999);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}