using System.Linq.Expressions;
using System.Transactions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using Microsoft.Extensions.Configuration;
using CapstoneProject.Application.Common.Helpers;

namespace CapstoneProject.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<AuthResponse>>
{
    private readonly IOtpCacheService _otpCacheService;
    private readonly ILogger<VerifyOtpCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly string _passwordEncryptKey;

    public VerifyOtpCommandHandler(IOtpCacheService otpCacheService, ILogger<VerifyOtpCommandHandler> logger,
        IUnitOfWork unitOfWork, IIdentityService identityService, IMapper mapper, IJwtService jwtService, IConfiguration configuration)
    {
        _otpCacheService = otpCacheService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _mapper = mapper;
        _jwtService = jwtService;
        _passwordEncryptKey = configuration.GetValue<string>("PasswordEncryptKey") ?? throw new Exception("PasswordEncryptKey is not set");
    }

    public async Task<Result<AuthResponse>> Handle(VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        try
        {
            //1. Verify OTP
            var otpResult = _otpCacheService.VerifyOtp(command.Request.Contact, command.Request.Otp,
                command.Request.OtpType, command.Request.OtpSentChannel);
            if (!otpResult.Success)
            {
                return Result<AuthResponse>.Failure(otpResult.Message, ErrorCodeEnum.ValidationFailed);
            }

            //2. If OTP is valid, proceed with user registration or password reset based on OTP type
            var userData = otpResult.UserData;
            if (userData == null)
            {
                return Result<AuthResponse>.Failure("User data is missing after OTP verification.", ErrorCodeEnum.NotFound);
            }

            var result = command.Request.OtpType switch
            {
                OtpTypeEnum.Registration => await HandleVerfiyOtpForRegister(command, cancellationToken, userData),
                OtpTypeEnum.PasswordReset => await HandleVerfiyOtpForResetPassword(command, cancellationToken, userData),
                _ => Result<AuthResponse>.Failure("Invalid OTP type.", ErrorCodeEnum.ValidationFailed)
            };
            // remove OTP from cache and clear rate limiting tracker (no need to wait for the task to complete)
            if (result.IsSuccess)
            {
                _ = Task.Run(() =>
                {
                    _otpCacheService.RemoveOtp(command.Request.Contact, command.Request.OtpType);
                    _otpCacheService.ClearRateLimitTracker(command.Request.Contact); // Clear rate limiting after successful verification
                });
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for {Contact}", command.Request.Contact);
            return Result<AuthResponse>.Failure("An error occurred while verifying the OTP.", ErrorCodeEnum.InternalError);
        }
    }

    private async Task<Result<AuthResponse>> HandleVerfiyOtpForRegister(VerifyOtpCommand command, CancellationToken cancellationToken, object userData)
    {
        var registerRequest = (RegisterRequest)userData;
        //decrypt password
        registerRequest.Password = PasswordCryptoHelper.Decrypt(registerRequest.Password, _passwordEncryptKey);
        var user = _mapper.Map<AppUser>(registerRequest);
        user.Id = Guid.NewGuid();
        user.InitializeEntity(user.Id);
        using (var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(1)
            },
            TransactionScopeAsyncFlowOption.Enabled
        ))
        {
            var createResult = await _identityService.CreateUserAsync(user, registerRequest.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return Result<AuthResponse>.Failure("Failed to create user", ErrorCodeEnum.ValidationFailed, errors);
            }

            var roleResult = await _identityService.AddUserToRoleAsync(user, RoleEnum.Learner.ToString());
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description).ToList();
                return Result<AuthResponse>.Failure("Failed to add user to role", ErrorCodeEnum.ValidationFailed, errors);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            scope.Complete();
        }

        // Generate refresh token and save to user (for refresh-token flow later)
        var (refreshToken, refreshTokenExpiryTime) = _jwtService.GenerateRefreshTokenWithExpiration();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdateEntity(user.Id);
        await _identityService.UpdateUserAsync(user);

        // Generate JWT and return auth response so client does not need to login again
        var (token, roles, _, expiresAt) = _jwtService.GenerateJwtTokenWithExpiration(user);
        var authResponse = new AuthResponse
        {
            AccessToken = token,
            Roles = roles,
            ExpiresAt = expiresAt
        };

        return Result<AuthResponse>.Success(authResponse, "User registered successfully.");
    }

    private async Task<Result<AuthResponse>> HandleVerfiyOtpForResetPassword(VerifyOtpCommand command, CancellationToken cancellationToken, object userData)
    {
        // Extract password reset data from cached userData
        dynamic passwordResetData = userData;
        var encryptedPassword = (string)passwordResetData.EncryptedPassword;
        var resetToken = (string)passwordResetData.ResetToken;

        // Decrypt the password to get the plain text for Identity's ResetPasswordAsync
        var plainPassword = PasswordCryptoHelper.Decrypt(encryptedPassword, _passwordEncryptKey);

        Expression<Func<AppUser, bool>> expression = command.Request.OtpSentChannel switch
        {
            NotificationChannelEnum.Email => x => x.Email == command.Request.Contact,
            NotificationChannelEnum.SMS => x => x.PhoneNumber == command.Request.Contact,
            _ => throw new ArgumentException("Invalid notification channel.")
        };

        var updateResult = await _identityService.ResetUserPasswordAsync(expression, resetToken, plainPassword);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description).ToList();
            return Result<AuthResponse>.Failure("Failed to update user", ErrorCodeEnum.InternalError, errors);
        }

        return Result<AuthResponse>.Success(null!, "Password reset successfully.");
    }
}
