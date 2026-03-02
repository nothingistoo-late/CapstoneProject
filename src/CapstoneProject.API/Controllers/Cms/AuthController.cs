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
    /// Sample request:
    /// 
    ///     POST /api/cms/auth/login
    ///     {
    ///        "email": "admin@example.com",
    ///        "password": "Admin@123",
    ///        "grantType": 0
    ///     }
    ///     
    /// `grantType` default is 0 (Password)
    /// </remarks>
    /// <param name="request">Login request</param>
    /// <returns>User information and authentication token</returns>
    /// <response code="200">Login successfully</response>
    /// <response code="400">Login failed (validation error)</response>
    /// <response code="401">Login failed (email or password is incorrect)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    [HttpPost("login")]
    [ServiceFilter(typeof(AdminRoleAccessFilter))]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Login to the CMS system",
        Description = "This API is used for Authentication for CMS website",
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
    /// This API is used for Logging out from the CMS website. It will clear the refresh token and refresh token expiry time in the database.
    /// Need access token in the header.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/cms/auth/logout
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>Logout successfully</returns>
    /// <response code="200">Logout successfully</response>
    /// <response code="401">Logout failed (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    [HttpPost("logout")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Logout from the CMS system",
        Description = "This API is used for Logging out from the CMS website",
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
    /// Get profile of the logged-in user in cms system
    /// </summary>
    /// <remarks>
    /// This API retrieves the profile information of the currently authenticated user.
    /// It requires a valid access token in the request header.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/auth/profile
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>admin or Teacher profile information</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="401">Failed to retrieve profile (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="500">Failed to retrieve profile (internal server error)</response>
    [HttpGet("profile")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get profile of the logged-in user in cms system",
        Description = "This API retrieves the profile information of the currently cms authenticated user",
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
    /// Update profile of the logged-in user in cms system
    /// </summary>
    /// <remarks>
    /// This API updates the profile information of the currently authenticated user.
    /// It requires a valid access token in the request header.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/cms/auth/profile
    ///     Content-Type: multipart/form-data
    /// 
    /// Form fields (camelCase naming):
    /// - firstName (optional): First name (max 50 characters)
    /// - lastName (optional): Last name (max 50 characters)
    /// - phoneNumber (optional): Phone number (10-11 digits)
    /// - avatarFile (optional): Avatar image file (max 10MB, .jpg/.jpeg/.png/.gif)
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>Updated profile information</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Failed to update profile (validation error)</response>
    /// <response code="401">Failed to update profile (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="500">Failed to update profile (internal server error)</response>
    [HttpPut("profile")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update profile of the logged-in user in cms system",
        Description = "This API updates the profile information of the currently cms authenticated user",
        OperationId = "UpdateProfile",
        Tags = new[] { "CMS" }
    )]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request, IFormFile? avatarFile)
    {
        var command = new UpdateProfileCommand(request, avatarFile);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Refresh token of the logged-in user in cms system
    /// </summary>
    /// <remarks>
    /// This API refresh access token of the currently cms authenticated user.
    /// It requires a valid access token in the request header.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/auth/refresh-token
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>refresh token for admin or staff</returns>
    /// <response code="200">Refresh token successfully</response>
    /// <response code="401">Failed to refresh token (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="500">Failed to refresh token (internal server error)</response>
    [HttpPost("refresh-token")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Refresh token for the logged-in user in cms system",
        Description = "This API refresh access token of the currently authenticated cms user",
        OperationId = "RefreshToken",
        Tags = new[] { "CMS" }
    )]
    public async Task<IActionResult> RefreshToken()
    {
        var query = new RefreshTokenCommand();
        var result = await _mediator.Send(query);
        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

}