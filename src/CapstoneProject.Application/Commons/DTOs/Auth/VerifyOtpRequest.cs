using System.Text.Json.Serialization;
using CapstoneProject.Application.Common.Enums;

namespace CapstoneProject.Application.Common.DTOs.Auth;

public class VerifyOtpRequest
{
    [JsonPropertyName("contact")]
    public string Contact { get; set; } = string.Empty;
    [JsonPropertyName("otp")]
    public string Otp { get; set; } = string.Empty;
    [JsonPropertyName("otpSentChannel")]
    public NotificationChannelEnum OtpSentChannel { get; set; } = NotificationChannelEnum.Email;
    [JsonPropertyName("otpType")]
    public OtpTypeEnum OtpType { get; set; } = OtpTypeEnum.Registration;
}