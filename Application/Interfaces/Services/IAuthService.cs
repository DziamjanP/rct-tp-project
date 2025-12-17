using CourseProject.Dtos;

namespace CourseProject.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<(string accessToken, string refreshToken)> RefreshAsync(string refreshToken);
}
