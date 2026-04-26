namespace CapstoneProject.Application.Common.Interfaces;

public interface IEntitlementService
{
    Task<bool> HasFeatureAsync(Guid userId, string featureKey, CancellationToken cancellationToken = default);
    Task<decimal?> GetNumericFeatureAsync(Guid userId, string featureKey, CancellationToken cancellationToken = default);
}

