using System.Transactions;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Google.Apis.Auth;
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
    private readonly GoogleSettings _googleSettings;

    public GoogleLoginCommandHandler(
        IIdentityService identityService,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleLoginCommandHandler> logger,
        IOptions<GoogleSettings> googleSettings)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _googleSettings = googleSettings.Value;
    }

    public async Task<Result<AuthResponse>> Handle(GoogleLoginCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Request.IdToken))
            return Result<AuthResponse>.Failure("IdToken là bắt buộc.", ErrorCodeEnum.InvalidInput);

        string? email = null;
        string? firstName = null;
        string? lastName = null;

        var googleToken = command.Request.IdToken.Trim();
        if (googleToken.StartsWith("ya29.", StringComparison.OrdinalIgnoreCase) || googleToken.StartsWith("1//", StringComparison.OrdinalIgnoreCase))
        {
            // Access token flow (same as EXE_BE): call Google userinfo API.
            var http = _httpClientFactory.CreateClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", googleToken);
            HttpResponseMessage response;
            try
            {
                response = await http.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Google userinfo request failed.");
                return Result<AuthResponse>.Failure("Mã thông báo Google không hợp lệ.", ErrorCodeEnum.InvalidCredentials);
            }

            if (!response.IsSuccessStatusCode)
                return Result<AuthResponse>.Failure("Mã thông báo Google không hợp lệ.", ErrorCodeEnum.InvalidCredentials);

            try
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var info = JsonSerializer.Deserialize<GoogleUserInfoResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                email = info?.Email;
                firstName = info?.GivenName;
                lastName = info?.FamilyName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse Google userinfo response.");
                return Result<AuthResponse>.Failure("Mã thông báo Google không hợp lệ.", ErrorCodeEnum.InvalidCredentials);
            }
        }
        else
        {
            // ID token flow: validate signature + audience.
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = !string.IsNullOrWhiteSpace(_googleSettings.ClientId)
                        ? new[] { _googleSettings.ClientId }
                        : null
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);
                email = payload.Email;
                firstName = payload.GivenName;
                lastName = payload.FamilyName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Google id_token validation failed.");
                return Result<AuthResponse>.Failure("Mã thông báo Google không hợp lệ.", ErrorCodeEnum.InvalidCredentials);
            }
        }

        if (string.IsNullOrWhiteSpace(email))
            return Result<AuthResponse>.Failure("Không tìm thấy email tài khoản Google.", ErrorCodeEnum.InvalidCredentials);

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
                FirstName = string.IsNullOrWhiteSpace(firstName) ? email.Split('@')[0] : firstName,
                LastName = lastName ?? string.Empty,
                JoiningAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
                CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow,
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
                    return Result<AuthResponse>.Failure("Không tạo được người dùng từ Google.", ErrorCodeEnum.ValidationFailed, errors);
                }
                var roleResult = await _identityService.AddUserToRoleAsync(user, RoleEnum.Learner.ToString());
                if (!roleResult.Succeeded)
                {
                    return Result<AuthResponse>.Failure("Không thể chỉ định vai trò.", ErrorCodeEnum.ValidationFailed);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                scope.Complete();
            }
        }

        // Avoid EF tracking conflicts: reload a tracked instance before update.
        var trackedUser = await _identityService.GetUserByIdAsync(user.Id.ToString());
        if (trackedUser == null)
            return Result<AuthResponse>.Failure("Không tìm thấy người dùng sau khi đăng nhập Google.", ErrorCodeEnum.InvalidCredentials);

        // Align with normal Login flow: issue refresh token so subsequent auth validation passes.
        var (refreshToken, refreshTokenExpiryTime) = _jwtService.GenerateRefreshTokenWithExpiration();
        trackedUser.LastLoginAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        trackedUser.RefreshToken = refreshToken;
        trackedUser.RefreshTokenExpiryTime = refreshTokenExpiryTime;
        await _identityService.UpdateUserAsync(trackedUser);

        var (token, roles, _, expiresAt) = _jwtService.GenerateJwtTokenWithExpiration(trackedUser);
        var authResponse = new AuthResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            Roles = roles,
            ExpiresAt = expiresAt
        };
        return Result<AuthResponse>.Success(authResponse, "Đăng nhập bằng Google thành công.");
    }

    private sealed class GoogleUserInfoResponse
    {
        public string? Email { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
    }
}



