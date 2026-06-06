using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;
using TaskManagement.API.Services;
using FluentAssertions;

namespace TaskManagement.Tests;

public class TaskServiceTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsOnlyUserTasks_WhenNotAdmin()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        context.Categories.Add(category);
        var user1 = new AppUser { Id = "user1", FullName = "User One", Email = "u1@test.com" };
        var user2 = new AppUser { Id = "user2", FullName = "User Two", Email = "u2@test.com" };
        context.Users.AddRange(user1, user2);
        context.Tasks.AddRange(
            new TaskItem { Title = "Task 1", OwnerId = "user1", CategoryId = 1, Owner = user1, Category = category },
            new TaskItem { Title = "Task 2", OwnerId = "user2", CategoryId = 1, Owner = user2, Category = category }
        );
        await context.SaveChangesAsync();

        var service = new TaskService(context, new Mock<ILogger<TaskService>>().Object);
        var tasks = await service.GetTasksAsync("user1", false);

        tasks.Should().HaveCount(1);
        tasks.First().Title.Should().Be("Task 1");
    }

    [Fact]
    public async Task DeleteTaskAsync_ByNonOwner_ThrowsUnauthorizedAccessException()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        context.Categories.Add(category);
        var owner = new AppUser { Id = "owner1", FullName = "Owner", Email = "owner@test.com" };
        context.Users.Add(owner);
        var task = new TaskItem { Id = 1, Title = "My Task", OwnerId = "owner1", CategoryId = 1 };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var service = new TaskService(context, new Mock<ILogger<TaskService>>().Object);
        var act = () => service.DeleteTaskAsync(1, "other-user", false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetTasksAsync_AsAdmin_ReturnsAllTasks()
    {
        var context = CreateInMemoryContext();
        var category = new Category { Id = 1, Name = "Dev" };
        context.Categories.Add(category);
        var u1 = new AppUser { Id = "u1", FullName = "U1", Email = "u1@test.com" };
        var u2 = new AppUser { Id = "u2", FullName = "U2", Email = "u2@test.com" };
        context.Users.AddRange(u1, u2);
        context.Tasks.AddRange(
            new TaskItem { Title = "T1", OwnerId = "u1", CategoryId = 1, Owner = u1, Category = category },
            new TaskItem { Title = "T2", OwnerId = "u2", CategoryId = 1, Owner = u2, Category = category }
        );
        await context.SaveChangesAsync();

        var service = new TaskService(context, new Mock<ILogger<TaskService>>().Object);
        var tasks = await service.GetTasksAsync("u1", true);

        tasks.Should().HaveCount(2);
    }
}