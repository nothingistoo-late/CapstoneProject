using MediatR;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(ICurrentUserService currentUserService, IIdentityService identityService, ILogger<RefreshTokenCommandHandler> logger, IJwtService jwtService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
        _logger = logger;
        _jwtService = jwtService;
        _logger = logger;
    }
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null)
            {
                return Result<AuthResponse>.Failure("Not authorized", ErrorCodeEnum.Unauthorized);
            }

            var user = await _identityService.GetUserByIdAsync(userId.ToString() ?? throw new InvalidOperationException("User ID is null"));
            if (user == null)
            {
                return Result<AuthResponse>.Failure("User not found", ErrorCodeEnum.NotFound);
            }
            var (token, roles, expiresInMinutes, expiresAt) = _jwtService.GenerateJwtTokenWithExpiration(user);
            var authResponse = new AuthResponse
            {
                AccessToken = token,
                Roles = roles,
                ExpiresAt = expiresAt
            };
            return Result<AuthResponse>.Success(authResponse, "Refresh token successfully!");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for user {UserId}", _currentUserService.UserId);
            return Result<AuthResponse>.Failure("Error refreshing token", ErrorCodeEnum.InternalError);
        }
    }
}