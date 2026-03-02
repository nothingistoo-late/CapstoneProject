using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Common.DTOs.Auth;

public class RegisterRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    [JsonPropertyName("confirmPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // [JsonPropertyName("grantType")]
    // public GrantTypeEnum GrantType { get; set; } = GrantTypeEnum.Password;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;
    [JsonPropertyName("learnerCode")]
    public string? LearnerCode { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;
    [JsonPropertyName("gender")]
    public GenderEnum? Gender { get; set; }
    [JsonPropertyName("dateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [JsonPropertyName("otpSentChannel")]
    public NotificationChannelEnum? OtpSentChannel { get; set; } = NotificationChannelEnum.Email;
}