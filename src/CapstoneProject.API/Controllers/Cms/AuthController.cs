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
    /// Đăng nhập CMS bằng email và mật khẩu. Chỉ user có role Admin hoặc Moderator mới đăng nhập được; nếu không trả 403. Trả về access token và roles. Dùng token trong header Authorization cho các API CMS.
    ///
    ///     POST /api/cms/auth/login
    ///     { "email": "admin@example.com", "password": "Admin@123", "grantType": 0 }
    ///
    /// **Request body (LoginRequest):**
    /// - email (string, bắt buộc): Email đăng nhập. Định dạng email hợp lệ.
    /// - password (string, bắt buộc): Mật khẩu. Phải thỏa ràng buộc Identity (ít nhất 6 ký tự, chữ thường, chữ số).
    /// - grantType (int, tùy chọn): Loại grant. Giá trị: 0 = Password (mặc định).
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
    /// Đăng xuất CMS: xóa refresh token trong database. Chỉ cần gửi access token trong header; không có request body. User phải có role Admin hoặc Moderator.
    ///
    ///     POST /api/cms/auth/logout
    ///     Headers: Authorization: Bearer &lt;access_token&gt;
    ///
    /// **Request:** Không có body. Chỉ header Authorization với Bearer token nhận từ Login hoặc RefreshToken.
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
    /// Lấy thông tin profile của user CMS đang đăng nhập (Admin/Moderator): id, email, firstName, lastName, phoneNumber, avatarUrl, roles,... Không có request body hay query.
    ///
    ///     GET /api/cms/auth/profile
    ///     Headers: Authorization: Bearer &lt;access_token&gt;
    ///
    /// **Request:** Không có body, không có query. Chỉ header Authorization với Bearer token.
    /// **Response data (ProfileResponse):** id, email, firstName, lastName, phoneNumber, avatarUrl, roles, lastLoginAt,...
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
    /// Cập nhật thông tin profile CMS: firstName, lastName, phoneNumber, avatar. Gửi multipart/form-data. Chỉ cập nhật các field gửi lên. Yêu cầu Bearer token (Admin/Moderator).
    ///
    ///     PUT /api/cms/auth/profile
    ///     Content-Type: multipart/form-data
    ///     Form: firstName, lastName, phoneNumber, avatarFile (optional)
    ///     Headers: Authorization: Bearer &lt;access_token&gt;
    ///
    /// **Request (Form – UpdateProfileRequest):**
    /// - firstName (string, tùy chọn): Tên. Tối đa 50 ký tự.
    /// - lastName (string, tùy chọn): Họ. Tối đa 50 ký tự.
    /// - phoneNumber (string, tùy chọn): Số điện thoại. Định dạng SĐT, unique (trừ SĐT của chính user).
    /// - avatarFile (file, tùy chọn): File ảnh avatar. Hỗ trợ jpg, png,... Kích thước tối đa theo cấu hình.
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
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request, IFormFile? avatarFile)
    {
        var command = new UpdateProfileCommand(request, avatarFile);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Refresh token for the logged-in CMS user
    /// </summary>
    /// <remarks>
    /// Lấy access token mới khi token cũ sắp hết hạn. Client gửi Bearer token hiện tại trong header. Response trả về accessToken, expiresAt, roles. Chỉ Admin/Moderator. Không có request body.
    ///
    ///     POST /api/cms/auth/refresh-token
    ///     Headers: Authorization: Bearer &lt;access_token&gt;
    ///
    /// **Request:** Không có body. Chỉ header Authorization: Bearer với token hiện tại.
    /// **Response data (AuthResponse):** accessToken, expiresAt, roles.
    /// </remarks>
    /// <response code="200">New access token returned. Returns message and data (accessToken, expiresAt, roles).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">User is not a CMS member</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("refresh-token")]
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
        var query = new RefreshTokenCommand();
        var result = await _mediator.Send(query);
        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

}