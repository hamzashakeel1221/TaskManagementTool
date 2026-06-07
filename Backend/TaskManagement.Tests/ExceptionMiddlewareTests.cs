using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using TaskManagement.API.Middleware;

namespace TaskManagement.Tests;

public class ExceptionMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;

    public ExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(context.Response.Body).ReadToEndAsync();
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        var context = CreateHttpContext();
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns401()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new UnauthorizedAccessException("Access denied.");
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_ReturnsCorrectMessage()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new UnauthorizedAccessException("Access denied.");
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        body.Should().Contain("Access denied.");
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_Returns404()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new KeyNotFoundException("Task not found.");
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_ReturnsCorrectMessage()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new KeyNotFoundException("Task not found.");
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        body.Should().Contain("Task not found.");
    }

    [Fact]
    public async Task InvokeAsync_InvalidOperationException_Returns400()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new InvalidOperationException("Email already registered.");
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_InvalidOperationException_ReturnsCorrectMessage()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new InvalidOperationException("Email already registered.");
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        body.Should().Contain("Email already registered.");
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_Returns500()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new Exception("Something went wrong.");
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_ReturnsGenericMessage()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new Exception("Something went wrong.");
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        body.Should().Contain("An unexpected error occurred.");
    }

    [Fact]
    public async Task InvokeAsync_ResponseContentType_IsJson()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new KeyNotFoundException("Not found.");
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_ResponseBody_IsValidJson()
    {
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new KeyNotFoundException("Not found.");
        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        var body = await ReadResponseBody(context);
        var act = () => JsonSerializer.Deserialize<object>(body);
        act.Should().NotThrow();
    }
}