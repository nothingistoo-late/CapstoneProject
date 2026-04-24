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
using CapstoneProject.Application.Features.Auth.Commands.GoogleLogin;

namespace CapstoneProject.API.Controllers.Learner;

/// <summary>
/// Controller quáº£n lÃ½ xÃ¡c thá»±c cho website Learner
/// </summary>
[ApiController]
[Route("api/learner/auth")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner")]
[SwaggerTag("This API is used for Authentication for Learner website")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login to the Learner website
    /// </summary>
    /// <remarks>
    /// ÄÄƒng nháº­p báº±ng email vÃ  máº­t kháº©u. Tráº£ vá» access token (JWT) vÃ  danh sÃ¡ch roles. DÃ¹ng token tráº£ vá» trong header Authorization cho cÃ¡c API cáº§n xÃ¡c thá»±c.
    ///
    /// **METHOD and path:** POST /api/learner/auth/login
    ///
    /// **Example request body:**
    ///     { "email": "user@example.com", "password": "User@123", "grantType": 0 }
    ///
    /// **Body (JSON):**
    /// - email (string, required): Email Ä‘Äƒng nháº­p. Äá»‹nh dáº¡ng email há»£p lá»‡.
    /// - password (string, required): Máº­t kháº©u. Pháº£i thá»a rÃ ng buá»™c Identity (Ã­t nháº¥t 6 kÃ½ tá»±, cÃ³ chá»¯ thÆ°á»ng, cÃ³ chá»¯ sá»‘).
    /// - grantType (int, optional): Loáº¡i grant. Possible values: 0 = Password (máº·c Ä‘á»‹nh). Hiá»‡n chá»‰ há»— trá»£ Password.
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
    /// Login with Google OAuth2
    /// </summary>
    /// <remarks>
    /// ÄÄƒng nháº­p báº±ng id_token tá»« Google Sign-In (client nháº­n tá»« Google sau khi user Ä‘Äƒng nháº­p Google). Server xÃ¡c thá»±c token, táº¡o hoáº·c cáº­p nháº­t user vÃ  tráº£ vá» JWT. Náº¿u user chÆ°a cÃ³ sáº½ Ä‘Æ°á»£c táº¡o vá»›i role Learner.
    ///
    /// **METHOD and path:** POST /api/learner/auth/google
    ///
    /// **Example request body:**
    ///     { "idToken": "eyJhbGciOiJSUzI1NiIs..." }
    ///
    /// **Body (JSON):**
    /// - idToken (string, required): ID token do Google tráº£ vá» sau khi user Ä‘Äƒng nháº­p Google (credential tá»« Google Sign-In). Server verify token vá»›i Google.
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
            return BadRequest(Result<AuthResponse>.Failure("Nội dung yêu cầu là bắt buộc", ErrorCodeEnum.ValidationFailed));
        var command = new GoogleLoginCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Logout from the Learner website
    /// </summary>
    /// <remarks>
    /// ÄÄƒng xuáº¥t: xÃ³a refresh token cá»§a user trong database. User cáº§n gá»­i access token hiá»‡n táº¡i trong header; khÃ´ng cÃ³ request body.
    ///
    /// **METHOD and path:** POST /api/learner/auth/logout
    ///
    /// **Body:** None. Headers only.
    ///
    /// **Headers:**
    /// - Authorization (required): Bearer &lt;access_token&gt; â€“ token nháº­n Ä‘Æ°á»£c tá»« Login / VerifyOtp / RefreshToken.
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
    /// Gá»­i thÃ´ng tin Ä‘Äƒng kÃ½ (multipart/form-data). Há»‡ thá»‘ng validate (email unique, máº­t kháº©u Ä‘á»§ rÃ ng buá»™c) rá»“i gá»­i OTP qua email. Sau Ä‘Ã³ gá»i verify-otp vá»›i OTP nháº­n Ä‘Æ°á»£c Ä‘á»ƒ hoÃ n táº¥t Ä‘Äƒng kÃ½ vÃ  nháº­n token.
    ///
    /// **METHOD and path:** POST /api/learner/auth/register
    ///
    /// **Example:** Content-Type: multipart/form-data. Form fields: email, password, confirmPassword, firstName, lastName, phoneNumber, learnerCode, gender, dateOfBirth
    ///
    /// **Body (Form â€“ multipart/form-data):**
    /// - email (string, required): Email Ä‘Äƒng kÃ½. Äá»‹nh dáº¡ng email, pháº£i chÆ°a tá»“n táº¡i trong há»‡ thá»‘ng.
    /// - password (string, required): Máº­t kháº©u. Ãt nháº¥t 6 kÃ½ tá»±, cÃ³ Ã­t nháº¥t 1 chá»¯ sá»‘, 1 chá»¯ thÆ°á»ng.
    /// - confirmPassword (string, required): XÃ¡c nháº­n máº­t kháº©u. Pháº£i trÃ¹ng vá»›i password.
    /// - firstName (string, required): TÃªn. Tá»‘i Ä‘a 50 kÃ½ tá»±.
    /// - lastName (string, required): Há». Tá»‘i Ä‘a 50 kÃ½ tá»±.
    /// - phoneNumber (string, required): Sá»‘ Ä‘iá»‡n thoáº¡i. Äá»‹nh dáº¡ng SÄT há»£p lá»‡, pháº£i unique.
    /// - learnerCode (string, optional): MÃ£ há»c viÃªn (náº¿u cÃ³).
    /// - gender (int?, optional): Giá»›i tÃ­nh. Possible values: 0 = Male, 1 = Female, 2 = Other (GenderEnum).
    /// - dateOfBirth (DateTime?, optional): NgÃ y sinh. ISO date.
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
    /// Gá»­i contact (email hoáº·c SÄT tÃ¹y kÃªnh) vÃ  máº­t kháº©u má»›i. Há»‡ thá»‘ng gá»­i OTP qua kÃªnh Ä‘Ã£ chá»n. Sau Ä‘Ã³ gá»i verify-otp vá»›i otpType = 2 (PasswordReset) vÃ  cÃ¹ng contact/otpSentChannel Ä‘á»ƒ hoÃ n táº¥t Ä‘á»•i máº­t kháº©u.
    ///
    /// **METHOD and path:** POST /api/learner/auth/reset-password
    ///
    /// **Example request body:**
    ///     { "contact": "user@example.com", "newPassword": "New@123", "otpSentChannel": 1 }
    ///
    /// **Body (JSON):**
    /// - contact (string, required): Email hoáº·c sá»‘ Ä‘iá»‡n thoáº¡i tÃ¹y otpSentChannel. Náº¿u channel Email thÃ¬ lÃ  email; náº¿u SMS thÃ¬ lÃ  SÄT. User pháº£i tá»“n táº¡i vá»›i contact nÃ y.
    /// - newPassword (string, required): Máº­t kháº©u má»›i. RÃ ng buá»™c giá»‘ng Ä‘Äƒng kÃ½ (Ã­t nháº¥t 8 kÃ½ tá»± cho reset, cÃ³ chá»¯ sá»‘, chá»¯ thÆ°á»ng).
    /// - otpSentChannel (int, required): KÃªnh gá»­i OTP. Possible values: 1 = Email, 2 = SMS. Pháº£i trÃ¹ng vá»›i contact (email vs SÄT).
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
    /// Sau khi Ä‘Äƒng kÃ½ (register) hoáº·c reset password, client nháº­n OTP qua email/SMS. Gá»i API nÃ y vá»›i OTP nháº­n Ä‘Æ°á»£c. Náº¿u otpType = 1 (Registration): táº¡o tÃ i khoáº£n vÃ  tráº£ vá» access token (khÃ´ng cáº§n login láº¡i). Náº¿u otpType = 2 (PasswordReset): chá»‰ Ä‘á»•i máº­t kháº©u, tráº£ vá» message, data = null.
    ///
    /// **METHOD and path:** POST /api/learner/auth/verify-otp
    ///
    /// **Example request body:**
    ///     { "contact": "user@example.com", "otp": "123456", "otpType": 1, "otpSentChannel": 1 }
    ///
    /// **Body (JSON):**
    /// - contact (string, required): Email hoáº·c SÄT â€“ cÃ¹ng giÃ¡ trá»‹ Ä‘Ã£ dÃ¹ng khi gá»i register hoáº·c reset-password. Äá»‹nh dáº¡ng pháº£i khá»›p vá»›i otpSentChannel (email náº¿u channel Email, SÄT náº¿u SMS).
    /// - otp (string, required): MÃ£ OTP 6 chá»¯ sá»‘ nháº­n qua email/SMS. Chá»‰ chá»¯ sá»‘, Ä‘Ãºng 6 kÃ½ tá»±.
    /// - otpType (int, required): Loáº¡i OTP. Possible values: 1 = Registration (xÃ¡c thá»±c Ä‘Äƒng kÃ½, sau khi verify táº¡o user vÃ  tráº£ token), 2 = PasswordReset (xÃ¡c thá»±c Ä‘á»•i máº­t kháº©u).
    /// - otpSentChannel (int, required): KÃªnh Ä‘Ã£ gá»­i OTP. Possible values: 1 = Email, 2 = SMS. Pháº£i trÃ¹ng vá»›i kÃªnh Ä‘Ã£ chá»n khi gá»i register/reset-password.
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
    /// Láº¥y thÃ´ng tin profile cá»§a user Ä‘ang Ä‘Äƒng nháº­p (email, firstName, lastName, phoneNumber, avatarUrl, learnerCode, gender, dateOfBirth,...). Cho phÃ©p role Learner hoáº·c Admin. KhÃ´ng cÃ³ request body, chá»‰ cáº§n Bearer token.
    ///
    /// **METHOD and path:** GET /api/learner/auth/profile
    ///
    /// **Body:** None. Headers only.
    ///
    /// **Headers:**
    /// - Authorization (required): Bearer &lt;access_token&gt; â€“ token nháº­n tá»« Login / VerifyOtp / RefreshToken.
    ///
    /// **Response data (ProfileResponse):** id, email, firstName, lastName, phoneNumber, avatarUrl, learnerCode, gender, dateOfBirth, lastLoginAt,...
    /// </remarks>
    /// <response code="200">Profile retrieved successfully. Returns message and data (profile).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Role is not allowed</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("profile")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get profile of the logged-in user",
        Description = "Returns profile (email, name, phone, avatar, etc.) of the authenticated user (Learner/Admin). Requires Bearer token.",
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
    /// Äá»•i máº­t kháº©u khi Ä‘Ã£ Ä‘Äƒng nháº­p. User gá»­i máº­t kháº©u hiá»‡n táº¡i vÃ  máº­t kháº©u má»›i (kÃ¨m xÃ¡c nháº­n). YÃªu cáº§u Bearer token (Learner).
    ///
    /// **METHOD and path:** POST /api/learner/auth/change-password
    ///
    /// **Example request body:**
    ///     { "currentPassword": "Old@123", "newPassword": "New@456", "confirmPassword": "New@456" }
    ///
    /// **Body (JSON):**
    /// - currentPassword (string, required): Máº­t kháº©u hiá»‡n táº¡i. Pháº£i Ä‘Ãºng vá»›i máº­t kháº©u trong DB thÃ¬ má»›i Ä‘á»•i Ä‘Æ°á»£c.
    /// - newPassword (string, required): Máº­t kháº©u má»›i. RÃ ng buá»™c: Ã­t nháº¥t 8 kÃ½ tá»±, cÃ³ chá»¯ sá»‘, chá»¯ thÆ°á»ng (theo Identity).
    /// - confirmPassword (string, required): XÃ¡c nháº­n máº­t kháº©u má»›i. Pháº£i trÃ¹ng vá»›i newPassword.
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
    /// Cáº­p nháº­t thÃ´ng tin profile: firstName, lastName, phoneNumber, gender, dateOfBirth, bio, avatar. Gá»­i dáº¡ng multipart/form-data. Chá»‰ cáº­p nháº­t cÃ¡c field gá»­i lÃªn. YÃªu cáº§u Bearer token (Learner).
    ///
    /// **METHOD and path:** PUT /api/learner/auth/profile
    ///
    /// **Example:** Content-Type: multipart/form-data. Form fields: firstName, lastName, phoneNumber, gender, dateOfBirth, bio, avatarFile
    ///
    /// **Body (Form â€“ multipart/form-data):**
    /// - firstName (string, optional): TÃªn. Tá»‘i Ä‘a 50 kÃ½ tá»±.
    /// - lastName (string, optional): Há». Tá»‘i Ä‘a 50 kÃ½ tá»±.
    /// - phoneNumber (string, optional): Sá»‘ Ä‘iá»‡n thoáº¡i. Äá»‹nh dáº¡ng SÄT, unique (trá»« SÄT cá»§a chÃ­nh user).
    /// - gender (int?, optional): Giá»›i tÃ­nh. 0 = Female, 1 = Male, 2 = Other.
    /// - dateOfBirth (DateTime?, optional): NgÃ y sinh (ISO date).
    /// - bio (string, optional): Giá»›i thiá»‡u báº£n thÃ¢n ngáº¯n, tá»‘i Ä‘a 500 kÃ½ tá»±.
    /// - avatarFile (file, optional): File áº£nh avatar. Há»— trá»£ jpg, png, ... KÃ­ch thÆ°á»›c tá»‘i Ä‘a theo cáº¥u hÃ¬nh (vd 10MB).
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
    /// Láº¥y access token má»›i khi token cÅ© sáº¯p háº¿t háº¡n. Client gá»­i request vá»›i Bearer token hiá»‡n táº¡i (hoáº·c refresh token tÃ¹y cáº¥u hÃ¬nh). Response tráº£ vá» accessToken, expiresAt, roles giá»‘ng Login. KhÃ´ng cÃ³ request body.
    ///
    /// **METHOD and path:** POST /api/learner/auth/refresh-token
    ///
    /// **Body:** None. Headers only.
    ///
    /// **Headers:**
    /// - Authorization (required): Bearer &lt;access_token&gt; hoáº·c refresh token. User pháº£i cÃ³ role Learner.
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
