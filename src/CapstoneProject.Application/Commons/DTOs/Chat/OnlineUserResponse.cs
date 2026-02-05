namespace CapstoneProject.Application.Commons.DTOs.Chat;

public class OnlineUserResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
    public bool IsOnline { get; set; }
}
