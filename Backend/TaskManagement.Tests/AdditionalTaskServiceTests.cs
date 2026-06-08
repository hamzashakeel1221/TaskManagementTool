using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;
using TaskStatus = TaskManagement.API.Models.TaskStatus;
using TaskManagement.API.Services;

namespace TaskManagement.Tests;

public class AdditionalTaskServiceTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static TaskService CreateService(AppDbContext context) =>
        new(context, new Mock<ILogger<TaskService>>().Object);

    // ─── GetTasksAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTasksAsync_ReturnsAssignedTasks_WhenNotAdmin()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        var owner = new AppUser { Id = "owner1", FullName = "Owner", Email = "owner@test.com" };
        var assignee = new AppUser { Id = "assignee1", FullName = "Assignee", Email = "assignee@test.com" };
        context.Categories.Add(category);
        context.Users.AddRange(owner, assignee);
        context.Tasks.Add(new TaskItem
        {
            Title = "Assigned Task",
            OwnerId = "owner1",
            AssignedToId = "assignee1",
            CategoryId = 1,
            Owner = owner,
            Category = category,
            AssignedTo = assignee
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var tasks = await service.GetTasksAsync("assignee1", false);

        tasks.Should().HaveCount(1);
        tasks.First().Title.Should().Be("Assigned Task");
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsEmpty_WhenNoTasks()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);

        var tasks = await service.GetTasksAsync("user1", false);

        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTasksAsync_OrdersByCreatedAtDescending()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        var owner = new AppUser { Id = "owner1", FullName = "Owner", Email = "owner@test.com" };
        context.Categories.Add(category);
        context.Users.Add(owner);
        context.Tasks.AddRange(
            new TaskItem { Title = "Old Task", OwnerId = "owner1", CategoryId = 1, Owner = owner, Category = category, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new TaskItem { Title = "New Task", OwnerId = "owner1", CategoryId = 1, Owner = owner, Category = category, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var tasks = (await service.GetTasksAsync("owner1", false)).ToList();

        tasks.First().Title.Should().Be("New Task");
        tasks.Last().Title.Should().Be("Old Task");
    }

    // ─── GetTaskByIdAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetTaskByIdAsync_AsAdmin_ReturnsAnyTask()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        var owner = new AppUser { Id = "owner1", FullName = "Owner", Email = "owner@test.com" };
        context.Categories.Add(category);
        context.Users.Add(owner);
        context.Tasks.Add(new TaskItem { Id = 1, Title = "Task", OwnerId = "owner1", CategoryId = 1, Owner = owner, Category = category });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetTaskByIdAsync(1, "admin1", true);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Task");
    }

    [Fact]
    public async Task GetTaskByIdAsync_AsAssignedUser_ReturnsTask()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        var owner = new AppUser { Id = "owner1", FullName = "Owner", Email = "owner@test.com" };
        var assignee = new AppUser { Id = "assignee1", FullName = "Assignee", Email = "assignee@test.com" };
        context.Categories.Add(category);
        context.Users.AddRange(owner, assignee);
        context.Tasks.Add(new TaskItem
        {
            Id = 1,
            Title = "Assigned Task",
            OwnerId = "owner1",
            AssignedToId = "assignee1",
            CategoryId = 1,
            Owner = owner,
            Category = category,
            AssignedTo = assignee
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetTaskByIdAsync(1, "assignee1", false);

        result.Should().NotBeNull();
    }

    // ─── CreateTaskAsync ──────────────────────────────────────────────────────

    [Fact]
    public Task CreateTaskAsync_WithInvalidCategory_ThrowsException()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);
        var dto = new CreateTaskDto("Task", "Desc", "High", 999, null, null);

        var act = () => service.CreateTaskAsync(dto, "owner1", false);

        return act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CreateTaskAsync_AsAdmin_AssignsToUser()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        var owner = new AppUser { Id = "admin1", FullName = "Admin", Email = "admin@test.com" };
        var assignee = new AppUser { Id = "user1", FullName = "User", Email = "user@test.com" };
        context.Categories.Add(category);
        context.Users.AddRange(owner, assignee);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateTaskDto("Admin Task", "Desc", "High", 1, null, "user1");
        var result = await service.CreateTaskAsync(dto, "admin1", true);

        result.AssignedToId.Should().Be("user1");
    }

    [Fact]
    public async Task CreateTaskAsync_AsRegularUser_IgnoresAssignedToId()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        var owner = new AppUser { Id = "user1", FullName = "User", Email = "user@test.com" };
        context.Categories.Add(category);
        context.Users.Add(owner);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateTaskDto("User Task", "Desc", "Low", 1, null, "someone-else");
        var result = await service.CreateTaskAsync(dto, "user1", false);

        result.AssignedToId.Should().BeNull();
    }

    [Fact]
    public async Task CreateTaskAsync_SetsCorrectOwner()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        var owner = new AppUser { Id = "owner1", FullName = "Owner", Email = "owner@test.com" };
        context.Categories.Add(category);
        context.Users.Add(owner);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateTaskDto("Task", "Desc", "Medium", 1, null, null);
        var result = await service.CreateTaskAsync(dto, "owner1", false);

        result.OwnerId.Should().Be("owner1");
    }

    // ─── UpdateTaskAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTaskAsync_ByOwner_UpdatesAllFields()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        var owner = new AppUser { Id = "owner1", FullName = "Owner", Email = "owner@test.com" };
        context.Categories.Add(category);
        context.Users.Add(owner);
        context.Tasks.Add(new TaskItem { Id = 1, Title = "Old Title", OwnerId = "owner1", CategoryId = 1, Status = TaskStatus.Pending });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new UpdateTaskDto("New Title", "New Desc", "High", "Completed", 1, null, null);
        var result = await service.UpdateTaskAsync(1, dto, "owner1", false);

        result.Title.Should().Be("New Title");
        result.Status.Should().Be("Completed");
        result.Priority.Should().Be("High");
    }

    [Fact]
    public async Task UpdateTaskAsync_ByNonOwnerNonAssigned_ThrowsUnauthorizedAccessException()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        var owner = new AppUser { Id = "owner1", FullName = "Owner", Email = "owner@test.com" };
        context.Categories.Add(category);
        context.Users.Add(owner);
        context.Tasks.Add(new TaskItem { Id = 1, Title = "Task", OwnerId = "owner1", CategoryId = 1 });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new UpdateTaskDto(null, null, null, "Pending", null, null, null);
        var act = () => service.UpdateTaskAsync(1, dto, "other-user", false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateTaskAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        var context = CreateInMemoryContext();
        var service = CreateService(context);
        var dto = new UpdateTaskDto(null, null, null, "Pending", null, null, null);

        var act = () => service.UpdateTaskAsync(999, dto, "owner1", false);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Task 999 not found.");
    }

    [Fact]
    public async Task UpdateTaskAsync_ByAssignedUser_UpdatesStatusOnly()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        var owner = new AppUser { Id = "owner1", FullName = "Owner", Email = "owner@test.com" };
        var assignee = new AppUser { Id = "assignee1", FullName = "Assignee", Email = "assignee@test.com" };
        context.Categories.Add(category);
        context.Users.AddRange(owner, assignee);
        context.Tasks.Add(new TaskItem
        {
            Id = 1,
            Title = "Original Title",
            OwnerId = "owner1",
            AssignedToId = "assignee1",
            CategoryId = 1,
            Status = TaskStatus.Pending
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new UpdateTaskDto("New Title", null, null, "InProgress", null, null, null);
        var result = await service.UpdateTaskAsync(1, dto, "assignee1", false);

        result.Status.Should().Be("InProgress");
        result.Title.Should().Be("Original Title"); // title not changed by assignee
    }
}