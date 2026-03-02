using MediatR;
using Microsoft.AspNetCore.Mvc;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Models;
using Swashbuckle.AspNetCore.Annotations;
using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Application.Features.Auth.Commands.Login;
using CapstoneProject.Application.Features.Auth.Commands.Logout;
using CapstoneProject.Application.Features.Auth.Commands.Register;
using CapstoneProject.API.Attributes;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Application.Features.Auth.Queries.GetProfile;
using CapstoneProject.Application.Features.Auth.Commands.VerifyOtp;
using CapstoneProject.Application.Features.Auth.Commands.ResetPassword;
using CapstoneProject.Application.Features.Auth.Commands.ChangePassword;
using CapstoneProject.Application.Features.Auth.Commands.UpdateProfile;
using CapstoneProject.Application.Features.Auth.Commands.RefreshToken;
using CapstoneProject.Application.Features.Auth.Commands.QuickLogin;


namespace CapstoneProject.API.Controllers.Student;

/// <summary>
/// Controller quản lý xác thực cho website Student
/// </summary>
[ApiController]
[Route("api/student/auth")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Student")]
[SwaggerTag("This API is used for Authentication for Student website")]
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
    /// Login to the Student website
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/Student/auth/login
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
        Summary = "Login to the Student website",
        Description = "This API is used for Authentication for Student website",
        OperationId = "Login",
        Tags = new[] { "Student" }
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
    /// This API allows quick login using a configured quick code. It will automatically log in with a demo user account.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Student/auth/quick-login
    ///     {
    ///        "quickCode": "DEMO123"
    ///     }
    /// </remarks>
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
        Tags = new[] { "Student" }
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
    /// Logout from the Student website
    /// </summary>
    /// <remarks>
    /// This API is used for Logging out from the Student website. It will clear the refresh token and refresh token expiry time in the database.
    /// Need access token in the header.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Student/auth/logout
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>Logout successfully</returns>
    /// <response code="200">Logout successfully</response>
    /// <response code="401">Logout failed (not authorized)</response>
    /// <response code="500">Logout failed (internal server error)</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Logout from the Student website",
        Description = "This API is used for Logging out from the Student website",
        OperationId = "Logout",
        Tags = new[] { "Student" }
    )]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand();
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Register a new Student
    /// </summary>
    /// <remarks>
    /// This API is used for Registering a new Student. It will cache an OTP code and send it to the user's email or phone number for verification.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Student/auth/register
    ///     Content-Type: multipart/form-data
    ///     
    /// Form fields (camelCase naming):
    /// - email (required): Email address
    /// - password (required): Password (min 6 characters)
    /// - confirmPassword (required): Password confirmation
    /// - firstName (required): First name (max 50 characters)
    /// - lastName (required): Last name (max 50 characters)
    /// - phoneNumber (required): Phone number (10-11 digits)
    /// - gender (optional): Gender (0=Male, 1=Female, 2=Other)
    /// - dateOfBirth (optional): Date of birth (YYYY-MM-DD format)
    /// - studentCode (optional): Student code
    /// 
    /// Note: Use camelCase for form field names to maintain consistency with React client naming conventions.
    /// </remarks>
    /// <response code="200">Register successfully</response>
    /// <response code="400">Register failed (validation error)</response>
    /// <response code="500">Register failed (internal server error)</response>
    [HttpPost("register")]
    [SkipModelValidation]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Register a new Student",
        Description = "This API is used for Registering a new Student",
        OperationId = "Register",
        Tags = new[] { "Student" }
    )]
    public async Task<IActionResult> Register(
        [FromForm(Name = "email")] string email,
        [FromForm(Name = "password")] string password,
        [FromForm(Name = "confirmPassword")] string confirmPassword,
        [FromForm(Name = "firstName")] string firstName,
        [FromForm(Name = "lastName")] string lastName,
        [FromForm(Name = "studentCode")] string? studentCode,
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
            StudentCode = studentCode,
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
    /// This API is used for Resetting password. It will cache an OTP code and send it to the user's email or phone number for verification.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Student/auth/reset-password
    ///     {
    ///        "contact": "user@example.com",
    ///        "newPassword": "User@123",
    ///        "otpSentChannel": 1
    ///     }
    /// 
    /// `otpSentChannel` default is 1 (Email), 2 (Phone). 
    /// `newPassword` is required
    /// `contact` is required
    /// </remarks>
    /// <response code="200">Reset password successfully</response>
    /// <response code="400">Reset password failed (validation error)</response>
    /// <response code="500">Reset password failed (internal server error)</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Reset password",
        Description = "This API is used for Resetting password",
        OperationId = "ResetPassword",
        Tags = new[] { "Student" }
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
    /// This API is used for Verifying OTP for registration or password reset. It will verify the OTP code and register the user or reset the password.
    /// The system automatically handles security tokens internally for enhanced security.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Student/auth/verify-otp
    ///     {
    ///        "contact": "user@example.com",
    ///        "otp": "123456",
    ///        "otpType": 1,
    ///        "otpSentChannel": 1
    ///     }
    /// 
    /// `otpType` default is 1 (Registration), 2 (Password Reset)
    /// `otpSentChannel` default is 1 (Email), 2 (Phone)
    /// </remarks>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Verify OTP for registration",
        Description = "This API is used for Verifying OTP for registration",
        OperationId = "VerifyOtp",
        Tags = new[] { "Student" }
    )]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var command = new VerifyOtpCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
    
    /// <summary>
    /// Get profile of the logged-in user in Student website
    /// </summary>
    /// <remarks>
    /// This API retrieves the profile information of the currently authenticated user.
    /// It requires a valid access token in the request header.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/Student/auth/profile
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>Student profile information</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="401">Failed to retrieve profile (not authorized)</response>
    /// <response code="403">No access (user is not a Student)</response>
    /// <response code="500">Failed to retrieve profile (internal server error)</response>
    [HttpGet("profile")]
    [AuthorizeRoles("Student")]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get profile of the logged-in user in Student website",
        Description = "This API retrieves the profile information of the currently Student authenticated user",
        OperationId = "GetProfile",
        Tags = new[] { "Student" }
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
    /// This API is used for Changing password. It will change the password of the currently authenticated user.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Student/auth/change-password
    ///     {
    ///        "currentPassword": "User@123",
    ///        "newPassword": "User@123",
    ///        "confirmPassword": "User@123"
    ///     }
    /// </remarks>
    /// `currentPassword` is required
    /// `newPassword` is required
    /// `confirmPassword` is required
    /// <response code="200">Change password successfully</response>
    /// <response code="404">Change password failed (user not found)</response>
    /// <response code="400">Change password failed (validation error)</response>
    /// <response code="401">Change password failed (not authorized)</response>
    /// <response code="500">Change password failed (internal server error)</response>
    [HttpPost("change-password")]
    [AuthorizeRoles("Student")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Change password",
        Description = "This API is used for Changing password",
        OperationId = "ChangePassword",
        Tags = new[] { "Student" }
    )]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update profile of the logged-in student
    /// </summary>
    /// <remarks>
    /// This API updates the profile information of the currently authenticated student.
    /// It requires a valid access token in the request header.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/Student/auth/profile
    ///     Content-Type: multipart/form-data
    /// 
    /// Form fields (camelCase naming):
    /// - firstName (optional): First name (max 50 characters)
    /// - lastName (optional): Last name (max 50 characters)
    /// - phoneNumber (optional): Phone number (10-11 digits)
    /// - avatarFile (optional): Avatar image file (max 10MB, .jpg/.jpeg/.png/.gif)
    /// - studentProfile.studentCode (optional): Student code
    /// - studentProfile.dateOfBirth (optional): Date of birth (YYYY-MM-DD)
    /// - studentProfile.gender (optional): Gender (0=Male, 1=Female, 2=Other)
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>Updated profile information</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Failed to update profile (validation error)</response>
    /// <response code="401">Failed to update profile (not authorized)</response>
    /// <response code="500">Failed to update profile (internal server error)</response>
    [HttpPut("profile")]
    [AuthorizeRoles("Student")]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update profile of the logged-in student",
        Description = "This API updates the profile information of the currently authenticated student",
        OperationId = "UpdateProfile",
        Tags = new[] { "Student" }
    )] 
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request, IFormFile? avatarFile)
    {
        var command = new UpdateProfileCommand(request, avatarFile);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Refresh token of the logged-in user in Student website
    /// </summary>
    /// <remarks>
    /// This API refesh access token of the currently student user.
    /// It requires a valid access token in the request header.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/student/auth/refresh-token
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>refresh token for student</returns>
    /// <response code="200">Refresh token successfully</response>
    /// <response code="401">Failed to refresh token (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="500">Failed to refresh token (internal server error)</response>
    [HttpPost("refresh-token")]
    [AuthorizeRoles(nameof(RoleEnum.Student))]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Refresh token for the logged-in user in Student website",
        Description = "This API refesh access token of the currently authenticated student user",
        OperationId = "RefreshToken",
        Tags = new[] { "Student" }
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