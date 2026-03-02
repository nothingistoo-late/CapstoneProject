using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.User;

public class BatchUpdateUserStatusRequest
{
    public List<Guid> UserIds { get; set; } = new();
    public EntityStatusEnum Status { get; set; }
}
