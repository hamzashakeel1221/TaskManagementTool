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

    // ─── Existing Tests ────────────────────────────────────────────────────────

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
    public async Task LoginAsync_WithInvalidCredentials_ReturnsNull()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("bad@test.com"))
            .ReturnsAsync((AppUser?)null);

        var result = await _authService.LoginAsync(new LoginDto("bad@test.com", "wrongpassword"));

        result.Should().BeNull();
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

    // ─── New Tests ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponseDto()
    {
        var user = new AppUser
        {
            Id = "user1",
            Email = "valid@test.com",
            FullName = "Valid User",
            UserName = "valid@test.com"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync("valid@test.com"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "Pass@123"))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        var result = await _authService.LoginAsync(new LoginDto("valid@test.com", "Pass@123"));

        result.Should().NotBeNull();
        result!.Email.Should().Be("valid@test.com");
        result.Role.Should().Be("User");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        var user = new AppUser
        {
            Id = "user1",
            Email = "valid@test.com",
            FullName = "Valid User",
            UserName = "valid@test.com"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync("valid@test.com"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "wrongpassword"))
            .ReturnsAsync(false);

        var result = await _authService.LoginAsync(new LoginDto("valid@test.com", "wrongpassword"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsAuthResponseDto()
    {
        var user = new AppUser
        {
            Id = "newuser1",
            Email = "newuser@test.com",
            FullName = "New User",
            UserName = "newuser@test.com"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync("newuser@test.com"))
            .ReturnsAsync((AppUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), "Pass@123"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<AppUser, string>((u, _) =>
            {
                u.Id = user.Id;
                u.FullName = user.FullName;
            });

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(new List<string> { "User" });

        var result = await _authService.RegisterAsync(new RegisterDto("New User", "newuser@test.com", "Pass@123"));

        result.Should().NotBeNull();
        result.Email.Should().Be("newuser@test.com");
        result.Token.Should().NotBeNullOrEmpty();
        result.Role.Should().Be("User");
    }
}