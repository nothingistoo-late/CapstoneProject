using System.Transactions;
using MediatR;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<AuthResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleLoginCommandHandler> _logger;

    public GoogleLoginCommandHandler(
        IIdentityService identityService,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleLoginCommandHandler> logger)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(GoogleLoginCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Request.IdToken))
            return Result<AuthResponse>.Failure("IdToken is required.", ErrorCodeEnum.InvalidInput);

        // Validate Google id_token via tokeninfo endpoint
        var http = _httpClientFactory.CreateClient();
        var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(command.Request.IdToken)}";
        HttpResponseMessage response;
        try
        {
            response = await http.GetAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Google tokeninfo request failed.");
            return Result<AuthResponse>.Failure("Invalid Google token.", ErrorCodeEnum.InvalidCredentials);
        }

        if (!response.IsSuccessStatusCode)
            return Result<AuthResponse>.Failure("Invalid Google token.", ErrorCodeEnum.InvalidCredentials);

        string json;
        try
        {
            json = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            return Result<AuthResponse>.Failure("Invalid Google token.", ErrorCodeEnum.InvalidCredentials);
        }

        // Parse email and name from tokeninfo (simplified - in production use System.Text.Json)
        var email = System.Text.Json.JsonDocument.Parse(json).RootElement.TryGetProperty("email", out var e) ? e.GetString() : null;
        var name = System.Text.Json.JsonDocument.Parse(json).RootElement.TryGetProperty("name", out var n) ? n.GetString() : null;
        if (string.IsNullOrEmpty(email))
            return Result<AuthResponse>.Failure("Google account email not found.", ErrorCodeEnum.InvalidCredentials);

        var user = await _identityService.GetUserByFirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            // Create new user and assign Learner role
            user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = name ?? email.Split('@')[0],
                LastName = "",
                JoiningAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Status = Domain.Enums.EntityStatusEnum.Active
            };
            var password = Guid.NewGuid().ToString("N") + "Aa1!"; // Identity requires password
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromMinutes(1) },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                var createResult = await _identityService.CreateUserAsync(user, password);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors?.Select(x => x.Description).ToList() ?? new List<string>();
                    return Result<AuthResponse>.Failure("Failed to create user from Google.", ErrorCodeEnum.ValidationFailed, errors);
                }
                var roleResult = await _identityService.AddUserToRoleAsync(user, RoleEnum.Learner.ToString());
                if (!roleResult.Succeeded)
                {
                    return Result<AuthResponse>.Failure("Failed to assign role.", ErrorCodeEnum.ValidationFailed);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                scope.Complete();
            }
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _identityService.UpdateUserAsync(user);

        var (token, roles, _, expiresAt) = _jwtService.GenerateJwtTokenWithExpiration(user);
        var authResponse = new AuthResponse
        {
            AccessToken = token,
            Roles = roles,
            ExpiresAt = expiresAt
        };
        return Result<AuthResponse>.Success(authResponse, "Login with Google successfully.");
    }
}
