using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NexusGrid.Shared.Exceptions;
using NexusGrid.UserService.Auth;
using NexusGrid.UserService.Models;
using NexusGrid.UserService.Repositories;
using NexusGrid.UserService.Services;
using Xunit;

namespace NexusGrid.UserService.Tests.Services;

public sealed class UserServiceTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ILogger<NexusGrid.UserService.Services.UserService>> _loggerMock;
    private readonly IUserService _sut;

    public UserServiceTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _loggerMock = new Mock<ILogger<NexusGrid.UserService.Services.UserService>>();

        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "test-secret-key-min-32-characters-long!",
            Issuer = "test",
            Audience = "test",
            ExpirationMinutes = 60
        });

        _sut = new NexusGrid.UserService.Services.UserService(
            _repositoryMock.Object,
            _jwtTokenServiceMock.Object,
            jwtSettings,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "password123", "John", "Doe");
        _repositoryMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test-jwt-token");
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(3600);
        result.User.Email.Should().Be("test@example.com");
        result.User.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        var request = new RegisterRequest("taken@example.com", "password123", "John", "Doe");
        _repositoryMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task RegisterAsync_ShortPassword_ThrowsValidationException()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "short", "John", "Doe");

        // Act
        var act = () => _sut.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<Shared.Exceptions.ValidationException>()
            .WithMessage("*at least 8*");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = CreateSampleUser();
        user.PasswordHash = passwordHash;

        _repositoryMock.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(user))
            .Returns("test-jwt-token");

        // Act
        var result = await _sut.LoginAsync(new LoginRequest("test@example.com", "password123"));

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test-jwt-token");
        result.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsValidationException()
    {
        // Arrange
        var user = CreateSampleUser();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password");

        _repositoryMock.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = () => _sut.LoginAsync(new LoginRequest("test@example.com", "wrong-password"));

        // Assert
        await act.Should().ThrowAsync<Shared.Exceptions.ValidationException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ThrowsValidationException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByEmailAsync("nobody@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.LoginAsync(new LoginRequest("nobody@example.com", "password123"));

        // Assert
        await act.Should().ThrowAsync<Shared.Exceptions.ValidationException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUserDto()
    {
        // Arrange
        var user = CreateSampleUser();
        _repositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.GetByIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidRequest_ReturnsUpdatedUser()
    {
        // Arrange
        var user = CreateSampleUser();
        _repositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = new UpdateProfileRequest("Jane", "Smith");

        // Act
        var result = await _sut.UpdateProfileAsync(user.Id, request);

        // Assert
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        await _sut.DeleteAsync(id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static User CreateSampleUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
