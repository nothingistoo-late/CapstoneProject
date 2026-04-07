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
/// Controller quáº£n lÃ½ users cho CMS
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
    /// <remarks>
    /// Returns paginated users with optional filters. Admin only.
    ///
    /// **Query:**
    /// - page (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Items per page. Default 10.
    /// - search (string, optional): Search term.
    /// - email (string, optional): Filter by email.
    /// - phoneNumber (string, optional): Filter by phone.
    /// - role (string, optional): Filter by role (e.g. Learner, Admin, Moderator).
    /// - status (EntityStatusEnum?, optional): Active, Inactive.
    /// - joiningFrom (DateTime?, optional): Filter users joined from date.
    /// - joiningTo (DateTime?, optional): Filter users joined to date.
    /// - sortBy (string, optional), isAscending (bool?, optional).
    ///
    /// **METHOD and path:** GET /api/cms/users
    ///
    /// **Example request:** GET /api/cms/users?page=1&amp;pageSize=10&amp;role=Learner&amp;status=Active
    /// </remarks>
    /// <response code="200">Returns paginated list of users (data: items, totalCount, page, pageSize).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Admin only</response>
    /// <response code="500">Internal server error</response>
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
    /// <remarks>
    /// Returns full user details including roles and profile by user Id. Admin only.
    ///
    /// **Route:** id (Guid, required): User ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** GET /api/cms/users/{id}
    ///
    /// **Example request:** GET /api/cms/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
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
    /// <remarks>
    /// Creates a new user in the system. Admin only. Supports optional avatar upload (multipart/form-data).
    ///
    /// **Form (multipart/form-data):**
    /// - firstName (string, required), lastName (string, required), email (string, required), password (string, required).
    /// - phoneNumber (string, optional), role (RoleEnum, required): e.g. Admin=0, Learner=1, Moderator=2.
    /// - status (EntityStatusEnum?, optional): Inactive=0, Active=1. Defaults to Active.
    /// - avatarFile (file, optional): Avatar image.
    ///
    /// **METHOD and path:** POST /api/cms/users
    ///
    /// **Example:** Content-Type: multipart/form-data with firstName, lastName, email, password, role, optional avatarFile
    /// </remarks>
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
    /// <remarks>
    /// Updates user profile and roles by Id. Admin only. Optional avatar upload (multipart/form-data).
    ///
    /// **Route:** id (Guid, required): User ID.
    ///
    /// **Form (multipart/form-data):** firstName (string?, optional), lastName (string?, optional), email (string?, optional), phoneNumber (string?, optional), status (EntityStatusEnum), newRole (RoleEnum?, optional), avatarFile (file, optional).
    ///
    /// **METHOD and path:** PUT /api/cms/users/{id}
    ///
    /// **Example:** Content-Type: multipart/form-data with fields to update
    /// </remarks>
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
    /// <remarks>
    /// Soft-deletes a user by Id. Admin only. User record is marked deleted and excluded from normal queries.
    ///
    /// **Route:** id (Guid, required): User ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** DELETE /api/cms/users/{id}
    ///
    /// **Example request:** DELETE /api/cms/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
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

    /// <summary>Batch update user status (Active/Inactive).</summary>
    /// <remarks>
    /// Activates or deactivates multiple users by Id list. Admin only. Returns successCount, failedCount, notFoundIds.
    ///
    /// **Body (JSON):**
    /// - userIds (array of Guid, required): User IDs to update.
    /// - status (EntityStatusEnum, required): 0=Active, 1=Inactive.
    ///
    /// **METHOD and path:** POST /api/cms/users/batch/status
    ///
    /// **Example request body:** { "userIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ], "status": 0 }
    /// </remarks>
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
    /// <remarks>
    /// Deactivates QuickLogin users that have not logged in for the specified number of days. Also runs automatically daily via Hangfire. Admin only.
    ///
    /// **Query:** daysInactive (int, optional): Days of inactivity before deactivation. Default 7.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** POST /api/cms/users/quicklogin/cleanup
    ///
    /// **Example request:** POST /api/cms/users/quicklogin/cleanup?daysInactive=7
    /// </remarks>
    /// <response code="200">Returns message and data (deletedCount).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Admin only</response>
    /// <response code="500">Internal server error (cleanup failed)</response>
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
            return Ok(Result<int>.Success(deletedCount, $"Đã hủy kích hoạt thành công {deletedCount} người dùng QuickLogin không hoạt động"));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during QuickLogin cleanup");
            return StatusCode(500, Result<int>.Failure("Đã xảy ra lỗi trong quá trình dọn dẹp", ErrorCodeEnum.InternalError));
        }
    }
}
