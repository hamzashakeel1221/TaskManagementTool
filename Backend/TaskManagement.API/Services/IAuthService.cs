using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services;

    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }

