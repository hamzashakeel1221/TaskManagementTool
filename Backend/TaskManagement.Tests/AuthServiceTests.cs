using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;
using TaskManagement.API.Services;
using FluentAssertions;

namespace TaskManagement.Tests;

public class AuthServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["Jwt:Key"]).Returns("TestSecretKeyAtLeast32CharactersLong!!");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

        _loggerMock = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(_userManagerMock.Object, _configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("existing@test.com"))
            .ReturnsAsync(new AppUser { Email = "existing@test.com" });

        var act = () => _authService.RegisterAsync(new RegisterDto("Test User", "existing@test.com", "Pass@123"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already registered.");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ThrowsUnauthorizedAccessException()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("bad@test.com"))
            .ReturnsAsync((AppUser?)null);

        var act = () => _authService.LoginAsync(new LoginDto("bad@test.com", "wrongpassword"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task RegisterAsync_WithFailedIdentityResult_ThrowsInvalidOperationException()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

        var act = () => _authService.RegisterAsync(new RegisterDto("Test", "test@test.com", "weak"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Password too weak.");
    }
}