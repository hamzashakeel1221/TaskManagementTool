namespace TaskManagement.API.DTOs;
public record RegisterDto(string FullName, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Email, string FullName, string Role, DateTime Expiry);