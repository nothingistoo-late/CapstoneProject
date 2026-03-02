using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Features.Auth.Commands.Login;
using CapstoneProject.Application.Features.Auth.Commands.Logout;
using CapstoneProject.Application.Features.Auth.Commands.Register;
using CapstoneProject.Application.Features.Auth.Queries.GetProfile;
using CapstoneProject.Application.Features.Auth.Commands.VerifyOtp;
using CapstoneProject.Application.Features.Auth.Commands.ResetPassword;
using CapstoneProject.Application.Features.Auth.Commands.ChangePassword;
using CapstoneProject.Application.Features.Auth.Commands.UpdateProfile;
using CapstoneProject.Application.Features.Auth.Commands.RefreshToken;
using CapstoneProject.Application.Features.Auth.Commands.QuickLogin;
using CapstoneProject.Application.Features.Auth.Commands.GoogleLogin;

namespace CapstoneProject.API.Controllers.Learner;

/// <summary>
/// Controller quản lý xác thực cho website Learner
/// </summary>
[ApiController]
[Route("api/learner/auth")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner")]
[SwaggerTag("This API is used for Authentication for Learner website")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Login to the Learner website
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/learner/auth/login
    ///     {
    ///        "email": "user@example.com",
    ///        "password": "User@123",
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
    /// <response code="500">Login failed (internal server error)</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Login to the Learner website",
        Description = "This API is used for Authentication for Learner website. Returns JWT access token and refresh token.",
        OperationId = "Login",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Quick login with demo account
    /// </summary>
    /// <remarks>
    /// Allows quick login using a configured quick code. Used for testing/demo. Automatically logs in with a demo user.
    /// 
    ///     POST /api/learner/auth/quick-login
    ///     { "quickCode": "DEMO123" }
    /// </remarks>
    /// <response code="200">Login successfully</response>
    /// <response code="400">Invalid request or quick code</response>
    /// <response code="401">Quick code not valid</response>
    /// <response code="404">Demo user not found</response>
    [HttpPost("quick-login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Quick login with demo account",
        Description = "This API allows quick login using a configured quick code for testing purposes",
        OperationId = "QuickLogin",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> QuickLogin([FromBody] QuickLoginRequest request)
    {
        if (request == null)
        {
            return BadRequest(Result<AuthResponse>.Failure("Request body is required", Application.Common.Enums.ErrorCodeEnum.ValidationFailed));
        }
        _logger.LogInformation("QuickLogin endpoint called with QuickCode: {QuickCode}", request.QuickCode);
        var command = new QuickLoginCommand(request);
        var result = await _mediator.Send(command);
        _logger.LogInformation("QuickLogin result: IsSuccess={IsSuccess}, Message={Message}", result.IsSuccess, result.Message);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Đăng nhập bằng Google OAuth2 (gửi id_token từ Google Sign-In).
    /// </summary>
    /// <remarks>
    /// Client gửi id_token nhận được từ Google Sign-In. Server xác thực token và tạo/cập nhật user, trả về JWT.
    /// 
    ///     POST /api/learner/auth/google
    ///     { "idToken": "eyJhbGc..." }
    /// </remarks>
    /// <response code="200">Login successfully</response>
    /// <response code="400">Invalid or missing id_token</response>
    /// <response code="401">Token validation failed</response>
    [HttpPost("google")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(
        Summary = "Login with Google",
        Description = "Authenticate using Google OAuth2 id_token. Creates or updates user and returns JWT.",
        OperationId = "GoogleLogin",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        if (request == null)
            return BadRequest(Result<AuthResponse>.Failure("Request body is required", Application.Common.Enums.ErrorCodeEnum.ValidationFailed));
        var command = new GoogleLoginCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Logout from the Learner website
    /// </summary>
    /// <remarks>
    /// Clears refresh token in database. Requires access token in header.
    /// 
    ///     POST /api/learner/auth/logout
    ///     Headers: Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <response code="200">Logout successfully</response>
    /// <response code="401">Not authorized</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Logout from the Learner website",
        Description = "This API is used for Logging out from the Learner website",
        OperationId = "Logout",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand();
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Register a new Learner
    /// </summary>
    /// <remarks>
    /// Gửi thông tin đăng ký (multipart/form-data). Hệ thống lưu OTP và gửi qua email/phone. Sau đó gọi Verify OTP để hoàn tất.
    /// 
    ///     POST /api/learner/auth/register
    ///     Form: email, password, confirmPassword, firstName, lastName, phoneNumber, learnerCode (optional), gender, dateOfBirth
    /// </remarks>
    /// <response code="200">OTP sent successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [SkipModelValidation]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Register a new Learner",
        Description = "This API is used for Registering a new Learner",
        OperationId = "Register",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> Register(
        [FromForm(Name = "email")] string email,
        [FromForm(Name = "password")] string password,
        [FromForm(Name = "confirmPassword")] string confirmPassword,
        [FromForm(Name = "firstName")] string firstName,
        [FromForm(Name = "lastName")] string lastName,
        [FromForm(Name = "learnerCode")] string? learnerCode,
        [FromForm(Name = "phoneNumber")] string phoneNumber,
        [FromForm(Name = "gender")] GenderEnum? gender = null,
        [FromForm(Name = "dateOfBirth")] DateTime? dateOfBirth = null)
    {
        var request = new RegisterRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            LearnerCode = learnerCode,
            Gender = gender,
            DateOfBirth = dateOfBirth,
        };

        var command = new RegisterCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Reset password
    /// </summary>
    /// <remarks>
    /// Gửi contact (email/phone) và mật khẩu mới. Hệ thống gửi OTP xác thực, sau đó gọi Verify OTP (otpType = PasswordReset) để đổi mật khẩu.
    /// 
    ///     POST /api/learner/auth/reset-password
    ///     { "contact": "user@example.com", "newPassword": "New@123", "otpSentChannel": 1 }
    /// </remarks>
    /// <response code="200">OTP sent</response>
    /// <response code="400">Validation error</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Reset password",
        Description = "This API is used for Resetting password",
        OperationId = "ResetPassword",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Verify OTP for registration or password reset
    /// </summary>
    /// <remarks>
    /// Sau khi đăng ký hoặc reset password, client gửi OTP nhận được. otpType: 1 = Registration (tự động đăng ký + login), 2 = PasswordReset.
    /// 
    ///     POST /api/learner/auth/verify-otp
    ///     { "contact": "user@example.com", "otp": "123456", "otpType": 1, "otpSentChannel": 1 }
    /// </remarks>
    /// <response code="200">Verified; with registration returns auth tokens</response>
    /// <response code="400">Invalid OTP or validation error</response>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Verify OTP for registration",
        Description = "This API is used for Verifying OTP for registration",
        OperationId = "VerifyOtp",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var command = new VerifyOtpCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get profile of the logged-in user in Learner website
    /// </summary>
    /// <remarks>
    /// Lấy thông tin profile của user đang đăng nhập. Cần access token.
    /// 
    ///     GET /api/learner/auth/profile
    ///     Headers: Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>Learner profile (email, name, phone, avatar, etc.)</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">User is not Learner</response>
    [HttpGet("profile")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get profile of the logged-in user in Learner website",
        Description = "This API retrieves the profile information of the currently authenticated Learner user",
        OperationId = "GetProfile",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> GetProfile()
    {
        var query = new GetProfileQuery();
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Change password
    /// </summary>
    /// <remarks>
    /// Đổi mật khẩu khi đã đăng nhập. Cần currentPassword, newPassword, confirmPassword.
    /// 
    ///     POST /api/learner/auth/change-password
    ///     { "currentPassword": "...", "newPassword": "...", "confirmPassword": "..." }
    /// </remarks>
    /// <response code="200">Password changed</response>
    /// <response code="400">Validation or wrong current password</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">User not found</response>
    [HttpPost("change-password")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Change password",
        Description = "This API is used for Changing password",
        OperationId = "ChangePassword",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update profile of the logged-in learner
    /// </summary>
    /// <remarks>
    /// Cập nhật firstName, lastName, phoneNumber, avatar (multipart/form-data). Cần access token.
    /// 
    ///     PUT /api/learner/auth/profile
    ///     Form: firstName, lastName, phoneNumber, avatarFile (optional)
    /// </remarks>
    /// <response code="200">Profile updated</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    [HttpPut("profile")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update profile of the logged-in learner",
        Description = "This API updates the profile information of the currently authenticated learner",
        OperationId = "UpdateProfile",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request, IFormFile? avatarFile)
    {
        var command = new UpdateProfileCommand(request, avatarFile);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Refresh token of the logged-in user in Learner website
    /// </summary>
    /// <remarks>
    /// Gửi refresh token (qua cookie hoặc header) để nhận access token mới.
    /// 
    ///     POST /api/learner/auth/refresh-token
    ///     Headers: Authorization: Bearer &lt;access_token&gt; (hoặc refresh token tùy cấu hình)
    /// </remarks>
    /// <response code="200">New access token returned</response>
    /// <response code="401">Invalid or expired token</response>
    /// <response code="403">Not a Learner</response>
    [HttpPost("refresh-token")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Refresh token for the logged-in user in Learner website",
        Description = "This API refreshes access token of the currently authenticated learner user",
        OperationId = "RefreshToken",
        Tags = new[] { "Learner" }
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
