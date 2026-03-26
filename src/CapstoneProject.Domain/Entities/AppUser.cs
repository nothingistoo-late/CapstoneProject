using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CapstoneProject.Domain.Entities;

public class AppUser : IdentityUser<Guid>, IEntityLike
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public DateTime JoiningAt { get; set; } = CapstoneProject.Domain.Common.VietnamDateTime.Now;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? AvatarPath { get; set; }
    public GenderEnum? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public DateTime? CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public EntityStatusEnum Status { get; set; }
}
