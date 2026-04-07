using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(IIdentityService identityService, IJwtService jwtService, ILogger<LoginCommandHandler> logger)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _logger = logger;
                
    }
    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _identityService.AuthenticateAsync(command.Request);
            if (!result.IsSuccess)
            {
                return Result<AuthResponse>.Failure(result.Message?? "Thông tin xác thực không hợp lệ", ErrorCodeEnum.InvalidCredentials);
            }

            var user = result.Data!;
            //generate refresh token and update auth infor of user
            var (refreshToken, refreshTokenExpiryTime) = _jwtService.GenerateRefreshTokenWithExpiration();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
            user.LastLoginAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            user.UpdateEntity(user.Id);
            await _identityService.UpdateUserAsync(user);
            //generate jwt token
            var (token, roles, expiresInMinutes, expiresAt) = _jwtService.GenerateJwtTokenWithExpiration(user);
            var authResponse = new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                Roles = roles,
                ExpiresAt = expiresAt
            };
            return Result<AuthResponse>.Success(authResponse, "Đăng nhập thành công!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in");
            return Result<AuthResponse>.Failure("Đã xảy ra lỗi khi đăng nhập", ErrorCodeEnum.InternalError);
        }
    }
}


