using NexusGrid.UserService.Models;

namespace NexusGrid.UserService.Auth;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
