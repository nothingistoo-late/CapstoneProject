using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Helpers;

namespace CapstoneProject.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly IOtpCacheService _otpCacheService;
    private readonly INotificationFactory _notificationFactory;
    private readonly string _passwordEncryptKey;


    public RegisterCommandHandler(
        ILogger<RegisterCommandHandler> logger, 
        IOtpCacheService otpCacheService,
        INotificationFactory notificationFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _otpCacheService = otpCacheService;
        _notificationFactory = notificationFactory;
        _passwordEncryptKey = configuration.GetValue<string>("PasswordEncryptKey") ?? throw new Exception("PasswordEncryptKey is not set");
    }
    public async Task<Result> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        try
        {   
            // Determine contact based on channel
            var channel = command.Request.OtpSentChannel ?? NotificationChannelEnum.Email;
            var contact = channel == NotificationChannelEnum.Email ? 
                command.Request.Email : command.Request.PhoneNumber;

            if (string.IsNullOrEmpty(contact))
            {
                return Result.Failure("Thông tin liên hệ là bắt buộc", ErrorCodeEnum.ValidationFailed);
            }
            
            //encrypt password and clear confirm password
            command.Request.Password = PasswordCryptoHelper.Encrypt(command.Request.Password, _passwordEncryptKey);
            command.Request.ConfirmPassword = string.Empty;

            // Generate and store OTP with rate limiting check
            string otpCode;
            try
            {
                otpCode = _otpCacheService.GenerateAndStoreOtp(
                    contact, 
                    OtpTypeEnum.Registration, 
                    command.Request, 
                    channel);
            }
            catch (InvalidOperationException ex)
            {
                // Rate limiting error - return user-friendly message
                return Result.Failure(ex.Message, ErrorCodeEnum.TooManyRequests);
            }

            // Build minimal notification; EmailService will render by template
            var notification = new NotificationRequest
            {
                To = contact,
                Template = NotificationTemplateEnums.Otp,
                TemplateData = new Dictionary<string, object>
                {
                    ["otpCode"] = otpCode,
                    ["otpType"] = OtpTypeEnum.Registration.ToString(),
                }
            };

            // Get notification sender based on channel
            var notificationSender = _notificationFactory.GetSender(channel);

            // Create recipient info
            var recipient = new RecipientInfo
            {
                Email = channel == NotificationChannelEnum.Email ? contact : null,
                PhoneNumber = channel == NotificationChannelEnum.Email ? null : contact,
                FullName = $"{command.Request.FirstName} {command.Request.LastName}".Trim()
            };

            // Send notification
            var sendResult = await notificationSender.SendNotificationAsync(notification, recipient);

            if (sendResult.ChannelResults.Any(cr => cr.Success))
            {
                _logger.LogInformation("OTP sent successfully to {Contact} via {Channel} for registration", 
                    contact, channel);
                return Result.Success($"Đăng ký bắt đầu. Vui lòng xác minh OTP được gửi tới {channel.ToString().ToLower()} của bạn để hoàn tất quá trình đăng ký.");
            }
            else
            {
                _logger.LogError("Failed to send OTP to {Contact} via {Channel}", contact, channel);
                return Result.Failure("Không gửi được mã xác minh. Vui lòng thử lại.", ErrorCodeEnum.InternalError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi trong quá trình đăng ký");
            return Result.Failure("Lỗi trong quá trình đăng ký", ErrorCodeEnum.InternalError);
        }
    }
}