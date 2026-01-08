using CapstoneProject.Application.Common.DTOs;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.User;

public class UserListItem : BaseResponse
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarPath { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime JoiningAt { get; set; }
    public List<RoleEnum> Roles { get; set; } = new List<RoleEnum>();
}
