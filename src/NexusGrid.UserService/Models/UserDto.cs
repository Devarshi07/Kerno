namespace NexusGrid.UserService.Models;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

public sealed record LoginRequest(
    string Email,
    string Password
);

public sealed record AuthResponse(
    string Token,
    string TokenType,
    int ExpiresIn,
    UserDto User
);

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName
);
