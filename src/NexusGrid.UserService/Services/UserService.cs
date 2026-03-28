using Microsoft.Extensions.Options;
using NexusGrid.Shared.Exceptions;
using NexusGrid.Shared.Models;
using NexusGrid.UserService.Auth;
using NexusGrid.UserService.Models;
using NexusGrid.UserService.Repositories;

namespace NexusGrid.UserService.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repository,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new Shared.Exceptions.ValidationException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new Shared.Exceptions.ValidationException("Password must be at least 8 characters.");

        if (await _repository.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new ConflictException($"User with email '{request.Email}' already exists.", "EMAIL_TAKEN");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(user, cancellationToken);

        _logger.LogInformation("User {UserId} registered with email {Email}", user.Id, user.Email);

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse(
            token,
            "Bearer",
            _jwtSettings.ExpirationMinutes * 60,
            MapToDto(user)
        );
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new Shared.Exceptions.ValidationException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Shared.Exceptions.ValidationException("Invalid email or password.");

        _logger.LogInformation("User {UserId} logged in", user.Id);

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse(
            token,
            "Bearer",
            _jwtSettings.ExpirationMinutes * 60,
            MapToDto(user)
        );
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User", id);

        return MapToDto(user);
    }

    public async Task<PaginatedResponse<UserDto>> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var users = await _repository.GetAllAsync(page, pageSize, cancellationToken);
        var totalCount = await _repository.GetTotalCountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<UserDto>(
            users.Select(MapToDto).ToList(),
            page,
            pageSize,
            totalCount,
            totalPages
        );
    }

    public async Task<UserDto> UpdateProfileAsync(
        Guid id, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User", id);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        await _repository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User {UserId} profile updated", id);

        return MapToDto(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("User {UserId} deleted", id);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}
