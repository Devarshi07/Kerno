using NexusGrid.Shared.Models;
using NexusGrid.UserService.Models;

namespace NexusGrid.UserService.Services;

public interface IUserService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<UserDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
