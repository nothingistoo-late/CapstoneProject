using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Helpers;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IOtpCacheService _otpCacheService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;
    private readonly INotificationFactory _notificationFactory;
    private readonly string _passwordEncryptKey;
    public ResetPasswordCommandHandler(ILogger<ResetPasswordCommandHandler> logger, IOtpCacheService otpCacheService,
     IIdentityService identityService, INotificationFactory notificationFactory, IConfiguration configuration)
    {
        _otpCacheService = otpCacheService;
        _identityService = identityService;
        _logger = logger;
        _notificationFactory = notificationFactory;
        _passwordEncryptKey = configuration.GetValue<string >("PasswordEncryptKey") ?? throw new Exception("PasswordEncryptKey is not set");
    }

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // No need to check user existence here - will be checked when OTP is verified
            // This reduces database queries and improves performance
            Expression<Func<AppUser, bool>> expression = command.Request.OtpSentChannel switch
            {
                NotificationChannelEnum.Email => x => x.Email == command.Request.Contact,
                NotificationChannelEnum.SMS => x => x.PhoneNumber == command.Request.Contact,
                _ => throw new ArgumentException("Invalid notification channel.")
            };
            var user = await _identityService.GetUserByFirstOrDefaultAsync(expression);
            if (user == null)
            {
                return Result.Failure("Không tìm thấy người dùng", ErrorCodeEnum.NotFound);
            }
            //encrypt password
            var passwordEncrypt = PasswordCryptoHelper.Encrypt(command.Request.NewPassword, _passwordEncryptKey);
            
            //generate token for reset password
            var token = await _identityService.GeneratePasswordResetToken(user);
            
            // Create password reset data to store in cache
            var passwordResetData = new
            {
                EncryptedPassword = passwordEncrypt,
                ResetToken = token
            };

            //create cache for otp with rate limiting check
            string otp;
            try
            {
                otp = _otpCacheService.GenerateAndStoreOtp(command.Request.Contact, OtpTypeEnum.PasswordReset, passwordResetData, command.Request.OtpSentChannel);
            }
            catch (InvalidOperationException ex)
            {
                // Rate limiting error - return user-friendly message
                return Result.Failure(ex.Message, ErrorCodeEnum.TooManyRequests);
            }

            
            //build notification body
            var recipient = new RecipientInfo
            {
                Email = command.Request.OtpSentChannel == NotificationChannelEnum.Email ? command.Request.Contact : null,
                PhoneNumber = command.Request.OtpSentChannel == NotificationChannelEnum.SMS ? command.Request.Contact : null,
                FullName = user.FirstName + " " + user.LastName
            };
            // Build minimal notification; EmailService will render by template
            var notification = new NotificationRequest
            {
                To = command.Request.Contact,
                Template = NotificationTemplateEnums.Otp,
                TemplateData = new Dictionary<string, object>
                {
                    ["otpCode"] = otp,
                    ["otpType"] = OtpTypeEnum.PasswordReset.ToString(),
                }
            };
            //send notification
            var notificationSender = _notificationFactory.GetSender(command.Request.OtpSentChannel);
            var sendResult = await notificationSender.SendNotificationAsync(notification, recipient);
            if (sendResult.ChannelResults.Any(cr => cr.Success))
            {
                _logger.LogInformation("OTP sent successfully to {Contact} via {Channel} for reset password", 
                    command.Request.Contact, command.Request.OtpSentChannel.ToString().ToLower());                    
                return Result.Success($"Đã bắt đầu đặt lại mật khẩu. Vui lòng xác minh OTP được gửi tới {command.Request.OtpSentChannel.ToString().ToLower()} của bạn để hoàn tất quy trình đặt lại mật khẩu.");

            }
            else
            {
                    _logger.LogError("Failed to send OTP to {Contact} via {Channel}", command.Request.Contact, command.Request.OtpSentChannel.ToString().ToLower());
                    return Result.Failure("Không gửi được mã xác minh. Vui lòng thử lại.", ErrorCodeEnum.InternalError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi trong quá trình đặt lại mật khẩu");
            return Result.Failure("Lỗi trong quá trình đặt lại mật khẩu",  ErrorCodeEnum.InternalError);
        }
    }
}