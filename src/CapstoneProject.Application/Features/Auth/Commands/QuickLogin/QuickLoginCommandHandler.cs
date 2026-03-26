using System.Transactions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Auth.Commands.QuickLogin;

public class QuickLoginCommandHandler : IRequestHandler<QuickLoginCommand, Result<AuthResponse>>
{
    private readonly IJwtService _jwtService;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<QuickLoginCommandHandler> _logger;

    public QuickLoginCommandHandler(
        IJwtService jwtService,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<QuickLoginCommandHandler> logger)
    {
        _jwtService = jwtService;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(QuickLoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Quick login can be temporarily disabled via configuration
            var quickLoginEnabled = _configuration.GetSection("QuickLogin").GetValue<bool?>("Enabled") ?? true;
            if (!quickLoginEnabled)
            {
                _logger.LogInformation("Quick login attempted but feature is disabled");
                return Result<AuthResponse>.Failure("Quick login is temporarily disabled.", ErrorCodeEnum.Forbidden);
            }

            // Get quick login configuration
            var configuredQuickCode = _configuration.GetSection("QuickLogin").GetValue<string>("Code");
            var defaultPassword = _configuration.GetSection("QuickLogin").GetValue<string>("DemoUserPassword") ?? "Demo@123";

            if (string.IsNullOrWhiteSpace(configuredQuickCode))
            {
                _logger.LogWarning("Quick login is not configured properly");
                return Result<AuthResponse>.Failure("Quick login is not available", ErrorCodeEnum.InternalError);
            }

            // Validate quick code
            if (command.Request.QuickCode != configuredQuickCode)
            {
                _logger.LogWarning("Invalid quick code attempted: {QuickCode}", command.Request.QuickCode);
                return Result<AuthResponse>.Failure("Invalid quick code", ErrorCodeEnum.InvalidCredentials);
            }

            // Generate random user info for testing
            var timestamp = CapstoneProject.Domain.Common.VietnamDateTime.NowOffset.ToUnixTimeMilliseconds();
            var guid = Guid.NewGuid().ToString("N")[..8]; // First 8 chars of GUID
            var randomEmail = $"test-{timestamp}-{guid}@quicklogin.test";
            var randomFirstName = $"User{guid[..4].ToUpper()}"; // First 4 chars as name
            var randomLastName = "Test";
            var randomPassword = $"QuickLogin@{Guid.NewGuid().ToString("N")[..8]}"; // Random password

            _logger.LogInformation("Quick login: Creating user in database - Email: {Email}", randomEmail);

            // Create user in database
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = randomEmail,
                UserName = randomEmail,
                FirstName = randomFirstName,
                LastName = randomLastName,
                JoiningAt = CapstoneProject.Domain.Common.VietnamDateTime.Now,
                Status = EntityStatusEnum.Active
            };
            user.InitializeEntity(user.Id);

            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(1)
                },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // Create user with Identity
                var createResult = await _identityService.CreateUserAsync(user, randomPassword);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.Select(e => e.Description).ToList();
                    _logger.LogError("Failed to create QuickLogin user: {Errors}", string.Join(", ", errors));
                    return Result<AuthResponse>.Failure("Failed to create user", ErrorCodeEnum.ValidationFailed, errors);
                }

                // Add Learner role
                var roleResult = await _identityService.AddUserToRoleAsync(user, RoleEnum.Learner.ToString());
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(e => e.Description).ToList();
                    _logger.LogError("Failed to add QuickLogin user to role: {Errors}", string.Join(", ", errors));
                    return Result<AuthResponse>.Failure("Failed to add user to role", ErrorCodeEnum.ValidationFailed, errors);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                scope.Complete();
            }

            // Generate JWT token from created user
            var (token, roles, expiresInMinutes, expiresAt) = _jwtService.GenerateJwtTokenWithExpiration(user);

            var authResponse = new AuthResponse
            {
                AccessToken = token,
                Roles = roles,
                ExpiresAt = expiresAt
            };

            _logger.LogInformation("Quick login successful - User created in database: {Email} (Name: {Name}, UserId: {UserId})", randomEmail, $"{randomFirstName} {randomLastName}", user.Id);
            return Result<AuthResponse>.Success(authResponse, "Quick login successful!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in quick login");
            return Result<AuthResponse>.Failure("An error occurred while quick logging in", ErrorCodeEnum.InternalError);
        }
    }
}

