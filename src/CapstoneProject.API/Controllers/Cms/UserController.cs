using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.Enums;
using Swashbuckle.AspNetCore.Annotations;
using CapstoneProject.Application.Features.User.Commands.BatchUpdateUserStatus;
using CapstoneProject.Application.Features.User.Commands.CreateUser;
using CapstoneProject.Application.Features.User.Commands.UpdateUser;
using CapstoneProject.Application.Features.User.Commands.DeleteUser;
using CapstoneProject.Application.Features.User.Queries.GetPagedUsers;
using CapstoneProject.Application.Features.User.Queries.GetUserById;
using CapstoneProject.Application.Commons.DTOs.User;
using BatchUpdateUserStatusResultDto = CapstoneProject.Application.Features.User.Commands.BatchUpdateUserStatus.BatchUpdateUserStatusResultDto;
using CapstoneProject.Application.Commons.Interfaces;

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
    [SwaggerOperation(Summary = "Get paginated list of users", Description = "Returns paginated users with optional filters: search, email, phoneNumber, role, status, joiningFrom, joiningTo, sortBy.", OperationId = "GetPagedUsers", Tags = new[] { "CMS" })]
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
    [SwaggerOperation(Summary = "Get user by ID", Description = "Returns full user details including roles and profile by user Id. Admin only.", OperationId = "GetUserById", Tags = new[] { "CMS" })]
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
    [SwaggerOperation(Summary = "Create a new user", Description = "Creates a new user in the system. Admin only. Supports optional avatar upload (multipart/form-data).", OperationId = "CreateUser", Tags = new[] { "CMS" })]
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
    [SwaggerOperation(Summary = "Update an existing user", Description = "Updates user profile and roles by Id. Admin only. Optional avatar upload (multipart/form-data).", OperationId = "UpdateUser", Tags = new[] { "CMS" })]
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
    [SwaggerOperation(Summary = "Delete a user (soft delete)", Description = "Soft-deletes a user by Id. Admin only. User record is marked deleted and excluded from normal queries.", OperationId = "DeleteUser", Tags = new[] { "CMS" })]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Batch cập nhật trạng thái user (Active/Inactive). Admin.</summary>
    [HttpPost("batch/status")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<BatchUpdateUserStatusResultDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Batch update user status", Description = "Activates or deactivates multiple users by Id list. Admin only. Returns successCount, failedCount and notFoundIds.", OperationId = "BatchUpdateUserStatus", Tags = new[] { "CMS" })]
    public async Task<IActionResult> BatchUpdateUserStatus([FromBody] BatchUpdateUserStatusRequest request)
    {
        var result = await _mediator.Send(new BatchUpdateUserStatusCommand(request.UserIds, request.Status));
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
