using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;

namespace TaskManagement.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<AppUser> userManager, IConfiguration config, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _config = config;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already registered.");

        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Registration failed for {Email}: {Errors}", dto.Email, errors);
            throw new InvalidOperationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "User");
        _logger.LogInformation("New user registered: {Email}", dto.Email);
        return await GenerateTokenAsync(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
            return null;
        }

        return await GenerateTokenAsync(user);
    }

    private async Task<AuthResponseDto> GenerateTokenAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var expireMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "60");
        var expiry = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new AuthResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Email!,
            user.FullName,
            role,
            expiry,
            user.Id
        );
    }
}

