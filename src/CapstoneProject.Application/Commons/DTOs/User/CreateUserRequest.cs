using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.User;

public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public RoleEnum Role { get; set; }
    /// <summary>Optional. Defaults to Active when not provided; BE assigns.</summary>
    public EntityStatusEnum? Status { get; set; }
}
