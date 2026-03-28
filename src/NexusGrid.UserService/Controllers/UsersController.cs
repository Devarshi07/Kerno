using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusGrid.Shared.Models;
using NexusGrid.UserService.Models;
using NexusGrid.UserService.Services;

namespace NexusGrid.UserService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdFromClaims();
        var result = await _userService.GetByIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<UserDto>>> GetAllAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> UpdateProfileAsync(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdFromClaims();
        var result = await _userService.UpdateProfileAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _userService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private Guid GetUserIdFromClaims()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("Invalid token claims.");

        return userId;
    }
}
