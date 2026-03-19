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
    /// Đăng nhập bằng email và mật khẩu. Trả về access token (JWT) và danh sách roles. Dùng token trả về trong header Authorization cho các API cần xác thực.
    ///
    /// **METHOD and path:** POST /api/learner/auth/login
    ///
    /// **Example request body:**
    ///     { "email": "user@example.com", "password": "User@123", "grantType": 0 }
    ///
    /// **Body (JSON):**
    /// - email (string, required): Email đăng nhập. Định dạng email hợp lệ.
    /// - password (string, required): Mật khẩu. Phải thỏa ràng buộc Identity (ít nhất 6 ký tự, có chữ thường, có chữ số).
    /// - grantType (int, optional): Loại grant. Possible values: 0 = Password (mặc định). Hiện chỉ hỗ trợ Password.
    /// </remarks>
    /// <response code="200">Login successfully. Returns message and data (accessToken, expiresAt, roles).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Email or password is incorrect</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Login to the Learner website",
        Description = "Authenticate by email and password. Returns JWT access token and roles.",
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
    /// Đăng nhập nhanh bằng quick code (dùng cho demo/test). Không cần email/password. Trả về access token tương tự Login. Cấu hình quick code trong appsettings.
    ///
    /// **METHOD and path:** POST /api/learner/auth/quick-login
    ///
    /// **Example request body:**
    ///     { "quickCode": "DEMO123" }
    ///
    /// **Body (JSON):**
    /// - quickCode (string, required): Mã quick login. Tối thiểu 3 ký tự. Phải khớp với cấu hình trên server.
    /// </remarks>
    /// <response code="200">Login successfully. Returns message and data (accessToken, expiresAt, roles).</response>
    /// <response code="400">Invalid request or quick code</response>
    /// <response code="401">Quick code not valid</response>
    /// <response code="404">Demo user not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("quick-login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Quick login with demo account",
        Description = "Quick login using configured quick code for testing/demo. Returns JWT access token.",
        OperationId = "QuickLogin",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> QuickLogin([FromBody] QuickLoginRequest request)
    {
        if (request == null)
        {
            return BadRequest(Result<AuthResponse>.Failure("Request body is required", ErrorCodeEnum.ValidationFailed));
        }
        _logger.LogInformation("QuickLogin endpoint called with QuickCode: {QuickCode}", request.QuickCode);
        var command = new QuickLoginCommand(request);
        var result = await _mediator.Send(command);
        _logger.LogInformation("QuickLogin result: IsSuccess={IsSuccess}, Message={Message}", result.IsSuccess, result.Message);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Login with Google OAuth2
    /// </summary>
    /// <remarks>
    /// Đăng nhập bằng id_token từ Google Sign-In (client nhận từ Google sau khi user đăng nhập Google). Server xác thực token, tạo hoặc cập nhật user và trả về JWT. Nếu user chưa có sẽ được tạo với role Learner.
    ///
    /// **METHOD and path:** POST /api/learner/auth/google
    ///
    /// **Example request body:**
    ///     { "idToken": "eyJhbGciOiJSUzI1NiIs..." }
    ///
    /// **Body (JSON):**
    /// - idToken (string, required): ID token do Google trả về sau khi user đăng nhập Google (credential từ Google Sign-In). Server verify token với Google.
    /// </remarks>
    /// <response code="200">Login successfully. Returns message and data (accessToken, expiresAt, roles).</response>
    /// <response code="400">Invalid or missing id_token</response>
    /// <response code="401">Token validation failed</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("google")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(
        Summary = "Login with Google",
        Description = "Authenticate using Google OAuth2 id_token. Creates or updates user, returns JWT access token.",
        OperationId = "GoogleLogin",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        if (request == null)
            return BadRequest(Result<AuthResponse>.Failure("Request body is required", ErrorCodeEnum.ValidationFailed));
        var command = new GoogleLoginCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Logout from the Learner website
    /// </summary>
    /// <remarks>
    /// Đăng xuất: xóa refresh token của user trong database. User cần gửi access token hiện tại trong header; không có request body.
    ///
    /// **METHOD and path:** POST /api/learner/auth/logout
    ///
    /// **Body:** None. Headers only.
    ///
    /// **Headers:**
    /// - Authorization (required): Bearer &lt;access_token&gt; – token nhận được từ Login / VerifyOtp / RefreshToken.
    /// </remarks>
    /// <response code="200">Logout successfully. Returns message only.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Logout from the Learner website",
        Description = "Clears refresh token in database. Requires Bearer access token in header.",
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
    /// Gửi thông tin đăng ký (multipart/form-data). Hệ thống validate (email unique, mật khẩu đủ ràng buộc) rồi gửi OTP qua email. Sau đó gọi verify-otp với OTP nhận được để hoàn tất đăng ký và nhận token.
    ///
    /// **METHOD and path:** POST /api/learner/auth/register
    ///
    /// **Example:** Content-Type: multipart/form-data. Form fields: email, password, confirmPassword, firstName, lastName, phoneNumber, learnerCode, gender, dateOfBirth
    ///
    /// **Body (Form – multipart/form-data):**
    /// - email (string, required): Email đăng ký. Định dạng email, phải chưa tồn tại trong hệ thống.
    /// - password (string, required): Mật khẩu. Ít nhất 6 ký tự, có ít nhất 1 chữ số, 1 chữ thường.
    /// - confirmPassword (string, required): Xác nhận mật khẩu. Phải trùng với password.
    /// - firstName (string, required): Tên. Tối đa 50 ký tự.
    /// - lastName (string, required): Họ. Tối đa 50 ký tự.
    /// - phoneNumber (string, required): Số điện thoại. Định dạng SĐT hợp lệ, phải unique.
    /// - learnerCode (string, optional): Mã học viên (nếu có).
    /// - gender (int?, optional): Giới tính. Possible values: 0 = Male, 1 = Female, 2 = Other (GenderEnum).
    /// - dateOfBirth (DateTime?, optional): Ngày sinh. ISO date.
    /// </remarks>
    /// <response code="200">OTP sent successfully. Returns message only. Call verify-otp to complete registration.</response>
    /// <response code="400">Validation error (e.g. password constraints, email already exists)</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [SkipModelValidation]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Register a new Learner",
        Description = "Submit registration form. Sends OTP to email. Call verify-otp with received OTP to complete registration.",
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
    /// Gửi contact (email hoặc SĐT tùy kênh) và mật khẩu mới. Hệ thống gửi OTP qua kênh đã chọn. Sau đó gọi verify-otp với otpType = 2 (PasswordReset) và cùng contact/otpSentChannel để hoàn tất đổi mật khẩu.
    ///
    /// **METHOD and path:** POST /api/learner/auth/reset-password
    ///
    /// **Example request body:**
    ///     { "contact": "user@example.com", "newPassword": "New@123", "otpSentChannel": 1 }
    ///
    /// **Body (JSON):**
    /// - contact (string, required): Email hoặc số điện thoại tùy otpSentChannel. Nếu channel Email thì là email; nếu SMS thì là SĐT. User phải tồn tại với contact này.
    /// - newPassword (string, required): Mật khẩu mới. Ràng buộc giống đăng ký (ít nhất 8 ký tự cho reset, có chữ số, chữ thường).
    /// - otpSentChannel (int, required): Kênh gửi OTP. Possible values: 1 = Email, 2 = SMS. Phải trùng với contact (email vs SĐT).
    /// </remarks>
    /// <response code="200">OTP sent successfully. Returns message only. Call verify-otp (otpType=2) to complete.</response>
    /// <response code="400">Validation error</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Reset password",
        Description = "Request OTP for password reset. Send contact and new password. Then call verify-otp with otpType=2 to complete.",
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
    /// Sau khi đăng ký (register) hoặc reset password, client nhận OTP qua email/SMS. Gọi API này với OTP nhận được. Nếu otpType = 1 (Registration): tạo tài khoản và trả về access token (không cần login lại). Nếu otpType = 2 (PasswordReset): chỉ đổi mật khẩu, trả về message, data = null.
    ///
    /// **METHOD and path:** POST /api/learner/auth/verify-otp
    ///
    /// **Example request body:**
    ///     { "contact": "user@example.com", "otp": "123456", "otpType": 1, "otpSentChannel": 1 }
    ///
    /// **Body (JSON):**
    /// - contact (string, required): Email hoặc SĐT – cùng giá trị đã dùng khi gọi register hoặc reset-password. Định dạng phải khớp với otpSentChannel (email nếu channel Email, SĐT nếu SMS).
    /// - otp (string, required): Mã OTP 6 chữ số nhận qua email/SMS. Chỉ chữ số, đúng 6 ký tự.
    /// - otpType (int, required): Loại OTP. Possible values: 1 = Registration (xác thực đăng ký, sau khi verify tạo user và trả token), 2 = PasswordReset (xác thực đổi mật khẩu).
    /// - otpSentChannel (int, required): Kênh đã gửi OTP. Possible values: 1 = Email, 2 = SMS. Phải trùng với kênh đã chọn khi gọi register/reset-password.
    /// </remarks>
    /// <response code="200">Verified. For registration (otpType=1): returns message and data (accessToken, expiresAt, roles). For password reset (otpType=2): returns message only, data is null.</response>
    /// <response code="400">Invalid OTP or validation error</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Verify OTP for registration or password reset",
        Description = "Verify OTP. For registration (otpType=1): returns access token and roles so client does not need to login again. For password reset (otpType=2): returns success message only.",
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
    /// Get profile of the logged-in user
    /// </summary>
    /// <remarks>
    /// Lấy thông tin profile của Learner đang đăng nhập (email, firstName, lastName, phoneNumber, avatarUrl, learnerCode, gender, dateOfBirth,...). Chỉ user có role Learner. Không có request body, chỉ cần Bearer token.
    ///
    /// **METHOD and path:** GET /api/learner/auth/profile
    ///
    /// **Body:** None. Headers only.
    ///
    /// **Headers:**
    /// - Authorization (required): Bearer &lt;access_token&gt; – token nhận từ Login / VerifyOtp / RefreshToken.
    ///
    /// **Response data (ProfileResponse):** id, email, firstName, lastName, phoneNumber, avatarUrl, learnerCode, gender, dateOfBirth, lastLoginAt,...
    /// </remarks>
    /// <response code="200">Profile retrieved successfully. Returns message and data (profile).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">User is not Learner</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("profile")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get profile of the logged-in user",
        Description = "Returns profile (email, name, phone, avatar, etc.) of the authenticated Learner. Requires Bearer token.",
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
    /// Đổi mật khẩu khi đã đăng nhập. User gửi mật khẩu hiện tại và mật khẩu mới (kèm xác nhận). Yêu cầu Bearer token (Learner).
    ///
    /// **METHOD and path:** POST /api/learner/auth/change-password
    ///
    /// **Example request body:**
    ///     { "currentPassword": "Old@123", "newPassword": "New@456", "confirmPassword": "New@456" }
    ///
    /// **Body (JSON):**
    /// - currentPassword (string, required): Mật khẩu hiện tại. Phải đúng với mật khẩu trong DB thì mới đổi được.
    /// - newPassword (string, required): Mật khẩu mới. Ràng buộc: ít nhất 8 ký tự, có chữ số, chữ thường (theo Identity).
    /// - confirmPassword (string, required): Xác nhận mật khẩu mới. Phải trùng với newPassword.
    /// </remarks>
    /// <response code="200">Password changed successfully. Returns message only.</response>
    /// <response code="400">Validation error or wrong current password</response>
    /// <response code="401">Not authorized</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("change-password")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Change password",
        Description = "Change password for authenticated user. Requires current password, new password and confirm. Requires Bearer token.",
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
    /// Cập nhật thông tin profile: firstName, lastName, phoneNumber, gender, dateOfBirth, bio, avatar. Gửi dạng multipart/form-data. Chỉ cập nhật các field gửi lên. Yêu cầu Bearer token (Learner).
    ///
    /// **METHOD and path:** PUT /api/learner/auth/profile
    ///
    /// **Example:** Content-Type: multipart/form-data. Form fields: firstName, lastName, phoneNumber, gender, dateOfBirth, bio, avatarFile
    ///
    /// **Body (Form – multipart/form-data):**
    /// - firstName (string, optional): Tên. Tối đa 50 ký tự.
    /// - lastName (string, optional): Họ. Tối đa 50 ký tự.
    /// - phoneNumber (string, optional): Số điện thoại. Định dạng SĐT, unique (trừ SĐT của chính user).
    /// - gender (int?, optional): Giới tính. 0 = Female, 1 = Male, 2 = Other.
    /// - dateOfBirth (DateTime?, optional): Ngày sinh (ISO date).
    /// - bio (string, optional): Giới thiệu bản thân ngắn, tối đa 500 ký tự.
    /// - avatarFile (file, optional): File ảnh avatar. Hỗ trợ jpg, png, ... Kích thước tối đa theo cấu hình (vd 10MB).
    /// </remarks>
    /// <response code="200">Profile updated successfully. Returns message and data (updated profile).</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("profile")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update profile of the logged-in learner",
        Description = "Update profile (firstName, lastName, phoneNumber, gender, dateOfBirth, bio, avatar). Multipart form-data. Requires Bearer token.",
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
    /// Refresh token for the logged-in user
    /// </summary>
    /// <remarks>
    /// Lấy access token mới khi token cũ sắp hết hạn. Client gửi request với Bearer token hiện tại (hoặc refresh token tùy cấu hình). Response trả về accessToken, expiresAt, roles giống Login. Không có request body.
    ///
    /// **METHOD and path:** POST /api/learner/auth/refresh-token
    ///
    /// **Body:** None. Headers only.
    ///
    /// **Headers:**
    /// - Authorization (required): Bearer &lt;access_token&gt; hoặc refresh token. User phải có role Learner.
    ///
    /// **Response data (AuthResponse):** accessToken, expiresAt, roles.
    /// </remarks>
    /// <response code="200">New access token returned. Returns message and data (accessToken, expiresAt, roles).</response>
    /// <response code="401">Invalid or expired token</response>
    /// <response code="403">Not a Learner</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("refresh-token")]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = "JwtBearerAllowExpired")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Refresh token for the logged-in user",
        Description = "Returns new access token for the authenticated Learner. Requires Bearer token in header.",
        OperationId = "RefreshToken",
        Tags = new[] { "Learner" }
    )]
    public async Task<IActionResult> RefreshToken()
    {
        var command = new RefreshTokenCommand();
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
