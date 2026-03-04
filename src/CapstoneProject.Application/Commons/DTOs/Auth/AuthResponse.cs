namespace CapstoneProject.Application.Common.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}