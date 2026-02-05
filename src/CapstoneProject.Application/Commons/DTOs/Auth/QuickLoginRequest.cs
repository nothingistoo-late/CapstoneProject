using System.Text.Json.Serialization;

namespace CapstoneProject.Application.Common.DTOs.Auth;

public class QuickLoginRequest
{
    [JsonPropertyName("quickCode")]
    public string QuickCode { get; set; } = string.Empty;
}
