using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Common;

public interface IEntityLike
{
    Guid Id { get; set; }
    // audit
    DateTime? CreatedAt { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
    // status
    EntityStatusEnum Status { get; set; }
}


