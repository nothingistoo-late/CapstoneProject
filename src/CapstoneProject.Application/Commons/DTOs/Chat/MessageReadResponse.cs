namespace CapstoneProject.Application.Commons.DTOs.Chat;

public class MessageReadResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime ReadAt { get; set; }
}
