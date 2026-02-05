namespace CapstoneProject.Application.Commons.DTOs.Chat;

public class ChatRoomMemberResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public DateTime? LastReadAt { get; set; }
}
