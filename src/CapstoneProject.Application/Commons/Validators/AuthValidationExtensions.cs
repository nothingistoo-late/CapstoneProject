using FluentValidation;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Common.Validators;

public static class AuthValidationExtensions
{
    /// <summary>
    /// Validates email format
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email is required")
            .Must(email => string.IsNullOrWhiteSpace(email) || new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            .WithMessage("Invalid email format");
    }

    /// <summary>
    /// Validates email format and checks if email exists in database
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidEmailExists<T>(this IRuleBuilder<T, string> ruleBuilder, IUnitOfWork unitOfWork)
    {
        return ruleBuilder
            .ValidEmail()
            .MustAsync(async (email, cancellationToken) =>
            {
                var exists = await unitOfWork.Repository<AppUser>().AnyAsync(x => x.Email == email);
                return exists;
            })
            .WithMessage("Email not found");
    }

    /// <summary>
    /// Validates email format and checks if email is unique (not exists)
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidEmailUnique<T>(this IRuleBuilder<T, string> ruleBuilder, IUnitOfWork unitOfWork)
    {
        return ruleBuilder
            .ValidEmail()
            .MustAsync(async (email, cancellationToken) =>
            {
                var exists = await unitOfWork.Repository<AppUser>().AnyAsync(x => x.Email == email);
                return !exists;
            })
            .WithMessage("Email already exists");
    }

    /// <summary>
    /// Validates password with minimum length and Identity-aligned rules (digit, lowercase).
    /// Use this so OTP is only sent when password already satisfies backend Identity requirements.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> ruleBuilder, int minLength = 6)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(minLength).WithMessage($"Password must be at least {minLength} characters")
            .Must(p => p.Any(char.IsDigit)).WithMessage("Password must contain at least one digit.")
            .Must(p => p.Any(char.IsLower)).WithMessage("Password must contain at least one lowercase letter.");
    }

    /// <summary>
    /// Validates password confirmation matches password
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidConfirmPassword<T>(this IRuleBuilder<T, string> ruleBuilder, System.Linq.Expressions.Expression<Func<T, string>> passwordSelector)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(passwordSelector).WithMessage("Passwords do not match");
    }

    /// <summary>
    /// Validates phone number format
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Phone number is required")
            .Must(IsValidPhoneNumber).WithMessage("Invalid phone number format");
    }

    /// <summary>
    /// Validates phone number format and checks if phone is unique (not exists)
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidPhoneNumberUnique<T>(this IRuleBuilder<T, string> ruleBuilder, IUnitOfWork unitOfWork)
    {
        return ruleBuilder
            .ValidPhoneNumber()
            .MustAsync(async (phoneNumber, cancellationToken) =>
            {
                var exists = await unitOfWork.Repository<AppUser>().AnyAsync(x => x.PhoneNumber == phoneNumber);
                return !exists;
            })
            .WithMessage("Số điện thoại đã tồn tại");
    }

    /// <summary>
    /// Validates contact based on notification channel (Email or SMS)
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidContactByChannel<T>(this IRuleBuilder<T, string> ruleBuilder, 
        Func<T, NotificationChannelEnum> channelSelector)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Contact is required")
            .Must((model, contact) =>
            {
                var channel = channelSelector(model);
                return channel switch
                {
                    NotificationChannelEnum.Email => IsValidEmail(contact),
                    NotificationChannelEnum.SMS => IsValidPhoneNumber(contact),
                    _ => false
                };
            })
            .WithMessage((model, contact) =>
            {
                var channel = channelSelector(model);
                return channel switch
                {
                    NotificationChannelEnum.Email => "Contact must be a valid email address",
                    NotificationChannelEnum.SMS => "Contact must be a valid phone number",
                    _ => "Invalid notification channel"
                };
            });
    }

    /// <summary>
    /// Validates contact exists in database based on notification channel
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidContactExistsByChannel<T>(this IRuleBuilder<T, string> ruleBuilder,
        Func<T, NotificationChannelEnum> channelSelector, IUnitOfWork unitOfWork)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Contact is required")
            .MustAsync(async (model, contact, cancellationToken) =>
            {
                var channel = channelSelector(model);
                
                // First validate format - if invalid, return false immediately without DB query
                var isValidFormat = channel switch
                {
                    NotificationChannelEnum.Email => IsValidEmail(contact),
                    NotificationChannelEnum.SMS => IsValidPhoneNumber(contact),
                    _ => false
                };
                
                if (!isValidFormat)
                {
                    return false;
                }
                
                // Format is valid, now check existence in database
                if (channel == NotificationChannelEnum.Email)
                {
                    return await unitOfWork.Repository<AppUser>().AnyAsync(x => x.Email == contact);
                }
                else if (channel == NotificationChannelEnum.SMS)
                {
                    return await unitOfWork.Repository<AppUser>().AnyAsync(x => x.PhoneNumber == contact);
                }
                return false;
            })
            .WithMessage((model, contact) =>
            {
                var channel = channelSelector(model);
                
                // Check format first to provide appropriate error message
                var isValidFormat = channel switch
                {
                    NotificationChannelEnum.Email => IsValidEmail(contact),
                    NotificationChannelEnum.SMS => IsValidPhoneNumber(contact),
                    _ => false
                };
                
                if (!isValidFormat)
                {
                    return channel switch
                    {
                        NotificationChannelEnum.Email => "Contact must be a valid email address",
                        NotificationChannelEnum.SMS => "Contact must be a valid phone number",
                        _ => "Invalid notification channel"
                    };
                }
                
                // Format is valid but not found in database
                return channel switch
                {
                    NotificationChannelEnum.Email => "Email not found",
                    NotificationChannelEnum.SMS => "Phone number not found",
                    _ => "Contact not found"
                };
            });
    }

    /// <summary>
    /// Validates person name (first name, last name)
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidPersonName<T>(this IRuleBuilder<T, string> ruleBuilder, string fieldName, int maxLength = 50)
    {
        return ruleBuilder
            .NotEmpty().WithMessage($"{fieldName} is required")
            .MaximumLength(maxLength).WithMessage($"{fieldName} cannot exceed {maxLength} characters");
    }

    /// <summary>
    /// Validates contact based on notification channel without checking database existence
    /// Used for verify OTP where user data might be cached and not yet saved to database
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidContactByChannelOnly<T>(this IRuleBuilder<T, string> ruleBuilder,
        Func<T, NotificationChannelEnum> channelSelector)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Contact is required")
            .Must((model, contact) =>
            {
                var channel = channelSelector(model);
                return channel switch
                {
                    NotificationChannelEnum.Email => IsValidEmail(contact),
                    NotificationChannelEnum.SMS => IsValidPhoneNumber(contact),
                    _ => false
                };
            })
            .WithMessage((model, contact) =>
            {
                var channel = channelSelector(model);
                return channel switch
                {
                    NotificationChannelEnum.Email => "Contact must be a valid email address",
                    NotificationChannelEnum.SMS => "Contact must be a valid phone number",
                    _ => "Invalid notification channel"
                };
            });
    }

    /// <summary>
    /// Validates OTP code format
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidOtp<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("OTP is required")
            .Length(6).WithMessage("OTP must be 6 digits")
            .Matches(@"^\d{6}$").WithMessage("OTP must contain only digits");
    }

    #region Private Helper Methods

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove all non-digit characters except '+' at the beginning
        var cleanNumber = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d+]", "");
        
        // Check if it starts with + and has 10-15 digits, or just has 10-11 digits
        return System.Text.RegularExpressions.Regex.IsMatch(cleanNumber, @"^(\+\d{10,15}|\d{10,11})$");
    }

    #endregion
}
