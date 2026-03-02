namespace CapstoneProject.Application.Common.DTOs.Auth;

/// <summary>
/// Request đăng nhập bằng Google OAuth2 (frontend gửi id_token từ Google Sign-In).
/// </summary>
public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}
