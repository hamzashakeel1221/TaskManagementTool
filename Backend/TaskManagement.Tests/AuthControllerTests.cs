using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using TaskManagement.API.Controllers;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services;

namespace TaskManagement.Tests;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    // ─── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        var dto = new RegisterDto("Test User", "test@test.com", "Pass@123");
        var response = new AuthResponseDto("token123", "test@test.com", "Test User", "User", DateTime.UtcNow.AddHours(1), "user1");

        _authServiceMock.Setup(x => x.RegisterAsync(dto)).ReturnsAsync(response);

        var result = await _controller.Register(dto);

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().Be(response);
    }

    [Fact]
    public async Task Register_WhenEmailExists_ThrowsInvalidOperationException()
    {
        var dto = new RegisterDto("Test User", "existing@test.com", "Pass@123");
        _authServiceMock.Setup(x => x.RegisterAsync(dto))
            .ThrowsAsync(new InvalidOperationException("Email already registered."));

        var act = () => _controller.Register(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already registered.");
    }

    [Fact]
    public async Task Register_WhenPasswordTooWeak_ThrowsInvalidOperationException()
    {
        var dto = new RegisterDto("Test User", "test@test.com", "weak");
        _authServiceMock.Setup(x => x.RegisterAsync(dto))
            .ThrowsAsync(new InvalidOperationException("Password too weak."));

        var act = () => _controller.Register(dto);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ─── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        var dto = new LoginDto("test@test.com", "Pass@123");
        var response = new AuthResponseDto("token123", "test@test.com", "Test User", "User", DateTime.UtcNow.AddHours(1), "user1");

        _authServiceMock.Setup(x => x.LoginAsync(dto)).ReturnsAsync(response);

        var result = await _controller.Login(dto);

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().Be(response);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var dto = new LoginDto("bad@test.com", "wrongpass");
        _authServiceMock.Setup(x => x.LoginAsync(dto)).ReturnsAsync((AuthResponseDto?)null);

        var result = await _controller.Login(dto);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithNullResult_ReturnsUnauthorizedWithMessage()
    {
        var dto = new LoginDto("test@test.com", "wrongpass");
        _authServiceMock.Setup(x => x.LoginAsync(dto)).ReturnsAsync((AuthResponseDto?)null);

        var result = await _controller.Login(dto) as UnauthorizedObjectResult;

        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenInResponse()
    {
        var dto = new LoginDto("admin@test.com", "Admin@123");
        var response = new AuthResponseDto("jwt-token-here", "admin@test.com", "Admin", "Admin", DateTime.UtcNow.AddHours(1), "admin1");

        _authServiceMock.Setup(x => x.LoginAsync(dto)).ReturnsAsync(response);

        var result = await _controller.Login(dto) as OkObjectResult;
        var value = result!.Value as AuthResponseDto;

        value!.Token.Should().Be("jwt-token-here");
        value.Role.Should().Be("Admin");
    }
}