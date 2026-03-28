using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NexusGrid.UserService.Controllers;
using NexusGrid.UserService.Models;
using NexusGrid.UserService.Services;
using Xunit;

namespace NexusGrid.UserService.Tests.Controllers;

public sealed class AuthControllerTests
{
    private readonly Mock<IUserService> _serviceMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _serviceMock = new Mock<IUserService>();
        _sut = new AuthController(_serviceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_Returns201()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "password123", "John", "Doe");
        var authResponse = CreateSampleAuthResponse();
        _serviceMock.Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        var authResponse = CreateSampleAuthResponse();
        _serviceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.Token.Should().NotBeNullOrEmpty();
    }

    private static AuthResponse CreateSampleAuthResponse()
    {
        return new AuthResponse(
            "test-jwt-token",
            "Bearer",
            3600,
            new UserDto(
                Guid.NewGuid(),
                "test@example.com",
                "John",
                "Doe",
                "User",
                DateTime.UtcNow,
                DateTime.UtcNow
            )
        );
    }
}
