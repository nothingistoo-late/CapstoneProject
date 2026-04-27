using Microsoft.AspNetCore.Authorization;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Features.Auth.Commands.Login;
using CapstoneProject.Application.Features.Auth.Commands.Logout;
using CapstoneProject.Application.Features.Auth.Queries.GetProfile;
using CapstoneProject.Application.Features.Auth.Commands.UpdateProfile;
using CapstoneProject.Application.Features.Auth.Commands.RefreshToken;

namespace CapstoneProject.API.Controllers.Cms;

/// <summary>
/// Controller quản lý xác thực cho website CMS
/// </summary>
[ApiController]
[Route("api/cms/auth")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS")]
[SwaggerTag("This API is used for Authentication for CMS website")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login to the CMS system
    /// </summary>
    /// <remarks>
    /// Authenticate by email and password. Only Admin or Moderator can login; otherwise returns 403. Returns access token and roles. Use token in Authorization header for CMS APIs.
    ///
    /// **Body (JSON):**
    /// - email (string, required): Login email. Valid email format.
    /// - password (string, required): Password. Must satisfy Identity rules (min 6 chars, lowercase, digit).
    /// - grantType (int, optional): Grant type. Possible values: 0 = Password (default).
    ///
    /// **METHOD and path:** POST /api/cms/auth/login
    ///
    /// **Example request body:** { "email": "admin@example.com", "password": "Admin@123", "grantType": 0 }
    /// </remarks>
    /// <response code="200">Login successfully. Returns message and data (accessToken, expiresAt, roles).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Email or password is incorrect</response>
    /// <response code="403">User is not a CMS member (Admin/Moderator)</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [ServiceFilter(typeof(AdminRoleAccessFilter))]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Login to the CMS system",
        Description = "Authenticate by email and password. Returns JWT access token. CMS members (Admin/Moderator) only.",
        OperationId = "Login",
        Tags = new[] { "CMS" }
    )]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Logout from the CMS system
    /// </summary>
    /// <remarks>
    /// Clears refresh token in database. Send access token in header only; no request body. User must have Admin or Moderator role.
    ///
    /// **Body:** None. Headers only.
    ///
    /// **Headers:** Authorization (required): Bearer &lt;access_token&gt; – token from Login or RefreshToken.
    ///
    /// **METHOD and path:** POST /api/cms/auth/logout
    ///
    /// **Example request:** POST /api/cms/auth/logout with header Authorization: Bearer &lt;token&gt;
    /// </remarks>
    /// <response code="200">Logout successfully. Returns message only.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">User is not a CMS member</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("logout")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Logout from the CMS system",
        Description = "Clears refresh token in database. Requires Bearer token (Admin/Moderator).",
        OperationId = "Logout",
        Tags = new[] { "CMS" }
    )]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand();
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
    
    /// <summary>
    /// Get profile of the logged-in CMS user
    /// </summary>
    /// <remarks>
    /// Returns profile of the authenticated CMS user (Admin/Moderator): id, email, firstName, lastName, phoneNumber, avatarUrl, roles. Requires Bearer token.
    ///
    /// **Body:** None. **Query:** None. Headers only (Authorization: Bearer &lt;token&gt;).
    ///
    /// **METHOD and path:** GET /api/cms/auth/profile
    ///
    /// **Example request:** GET /api/cms/auth/profile
    /// </remarks>
    /// <response code="200">Profile retrieved successfully. Returns message and data (profile).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">User is not a CMS member</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("profile")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get profile of the logged-in CMS user",
        Description = "Returns profile of the authenticated CMS user (Admin/Moderator). Requires Bearer token.",
        OperationId = "GetProfile",
        Tags = new[] { "CMS" }
    )]
    public async Task<IActionResult> GetProfile()
    {
        var query = new GetProfileQuery();
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update profile of the logged-in CMS user
    /// </summary>
    /// <remarks>
    /// Updates CMS profile: firstName, lastName, phoneNumber, avatar. Send as multipart/form-data. Only sent fields are updated. Requires Bearer token (Admin/Moderator).
    ///
    /// **Form (multipart/form-data):**
    /// - firstName (string, optional): First name. Max 50 chars.
    /// - lastName (string, optional): Last name. Max 50 chars.
    /// - phoneNumber (string, optional): Phone number. Valid format, unique (except current user).
    /// - avatarFile (file, optional): Avatar image. jpg, png, etc. Max size per config.
    ///
    /// **METHOD and path:** PUT /api/cms/auth/profile
    ///
    /// **Example:** Content-Type: multipart/form-data with fields firstName, lastName, phoneNumber, avatarFile
    /// </remarks>
    /// <response code="200">Profile updated successfully. Returns message and data (updated profile).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">User is not a CMS member</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("profile")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update profile of the logged-in CMS user",
        Description = "Update profile (firstName, lastName, phoneNumber, avatar). Multipart form-data. Requires Bearer token.",
        OperationId = "UpdateProfile",
        Tags = new[] { "CMS" }
    )]
    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateProfileRequest request,
        IFormFile? avatarFile,
        IFormFile? coverImageFile)
    {
        var command = new UpdateProfileCommand(request, avatarFile, coverImageFile);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Refresh token for the logged-in CMS user
    /// </summary>
    /// <remarks>
    /// Returns new access token when current one is expiring. Send current Bearer token in header. Response: accessToken, expiresAt, roles. Admin/Moderator only. No request body.
    ///
    /// **Body:** None. Headers only (Authorization: Bearer &lt;token&gt;).
    ///
    /// **METHOD and path:** POST /api/cms/auth/refresh-token
    ///
    /// **Example request:** POST /api/cms/auth/refresh-token
    /// </remarks>
    /// <response code="200">New access token returned. Returns message and data (accessToken, expiresAt, roles).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">User is not a CMS member</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("refresh-token")]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = "JwtBearerAllowExpired")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Refresh token for the logged-in CMS user",
        Description = "Returns new access token for the authenticated CMS user. Requires Bearer token in header.",
        OperationId = "RefreshToken",
        Tags = new[] { "CMS" }
    )]
    public async Task<IActionResult> RefreshToken()
    {
        var command = new RefreshTokenCommand();
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

}