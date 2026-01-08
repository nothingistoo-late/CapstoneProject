namespace CapstoneProject.Application.Common.Models;

public class GoogleSettings
{    
    // OAuth2 settings (deprecated - chỉ dùng cho fallback nếu cần)
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}