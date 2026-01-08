using Microsoft.AspNetCore.Identity;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class AppRole : IdentityRole<Guid>, IEntityLike
{
    public string Description { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public EntityStatusEnum Status { get; set; }
}