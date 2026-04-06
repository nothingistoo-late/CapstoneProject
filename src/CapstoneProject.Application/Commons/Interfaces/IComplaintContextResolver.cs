using CapstoneProject.Application.Commons.Models.Complaints;

namespace CapstoneProject.Application.Common.Interfaces;

public interface IComplaintContextResolver
{
    Task<ComplaintContextResolvedDto?> ResolveAsync(
        string? contextType,
        Guid? contextId,
        string? contextDataJson,
    Guid? complaintUserId,
        CancellationToken cancellationToken);
}