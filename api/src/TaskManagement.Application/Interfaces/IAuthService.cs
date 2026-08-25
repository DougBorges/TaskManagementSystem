using TaskManagement.Application.Dtos;

namespace TaskManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(UserRegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(UserLoginDto dto, CancellationToken cancellationToken = default);
}
