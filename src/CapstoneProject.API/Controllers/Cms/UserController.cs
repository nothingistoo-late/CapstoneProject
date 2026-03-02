using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;
using Swashbuckle.AspNetCore.Annotations;
using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.API.Attributes;
using CapstoneProject.Application.Features.User.Commands.CreateUser;
using CapstoneProject.Application.Features.User.Commands.UpdateUser;
using CapstoneProject.Application.Features.User.Commands.DeleteUser;
using CapstoneProject.Application.Features.User.Queries.GetPagedUsers;
using CapstoneProject.Application.Features.User.Queries.GetUserById;
using CapstoneProject.Application.Commons.DTOs.User;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.API.Controllers.Cms;

/// <summary>
/// Controller quản lý users cho CMS
/// </summary>
[ApiController]
[Route("api/cms/users")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS")]
[SwaggerTag("This API is used for User Management in CMS")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IQuickLoginCleanupService _cleanupService;
    private readonly ILogger<UserController>? _logger;

    public UserController(IMediator mediator, IQuickLoginCleanupService cleanupService, ILogger<UserController>? logger = null)
    {
        _mediator = mediator;
        _cleanupService = cleanupService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of users
    /// </summary>
    [HttpGet]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(PaginationResult<UserListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get paginated list of users", OperationId = "GetPagedUsers", Tags = new[] { "CMS" })]
    public async Task<IActionResult> GetPagedUsers([FromQuery] UserFilter filter)
    {
        var query = new GetPagedUsersQuery(filter);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get user by ID", OperationId = "GetUserById", Tags = new[] { "CMS" })]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Create a new user", OperationId = "CreateUser", Tags = new[] { "CMS" })]
    public async Task<IActionResult> CreateUser([FromForm] CreateUserRequest request, IFormFile? avatarFile)
    {
        var command = new CreateUserCommand(request, avatarFile);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update an existing user", OperationId = "UpdateUser", Tags = new[] { "CMS" })]
    public async Task<IActionResult> UpdateUser(Guid id, [FromForm] UpdateUserRequest request, IFormFile? avatarFile)
    {
        var command = new UpdateUserCommand(id, request, avatarFile);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Delete a user (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete a user (soft delete)", OperationId = "DeleteUser", Tags = new[] { "CMS" })]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Cleanup inactive QuickLogin users (manual trigger)
    /// </summary>
    /// <param name="daysInactive">Number of days of inactivity before deletion (default: 7)</param>
    [HttpPost("quicklogin/cleanup")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Cleanup inactive QuickLogin users",
        Description = "Deactivates QuickLogin users that haven't logged in for the specified number of days. This job also runs automatically daily via Hangfire.",
        OperationId = "CleanupQuickLoginUsers",
        Tags = new[] { "CMS" }
    )]
    public async Task<IActionResult> CleanupQuickLoginUsers([FromQuery] int daysInactive = 7)
    {
        try
        {
            var deletedCount = await _cleanupService.CleanupInactiveUsersAsync(daysInactive);
            return Ok(Result<int>.Success(deletedCount, $"Successfully deactivated {deletedCount} inactive QuickLogin users"));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during QuickLogin cleanup");
            return StatusCode(500, Result<int>.Failure("An error occurred during cleanup", ErrorCodeEnum.InternalError));
        }
    }
}
