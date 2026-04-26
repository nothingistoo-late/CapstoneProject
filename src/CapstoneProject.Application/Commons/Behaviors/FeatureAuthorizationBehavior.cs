using MediatR;
using CapstoneProject.Application.Common.Attributes;
using CapstoneProject.Application.Common.Exceptions;
using CapstoneProject.Application.Common.Interfaces;

namespace CapstoneProject.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior to enforce feature-based entitlement on requests.
/// </summary>
public class FeatureAuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IEntitlementService _entitlementService;

    public FeatureAuthorizationBehavior(
        ICurrentUserService currentUserService,
        IEntitlementService entitlementService)
    {
        _currentUserService = currentUserService;
        _entitlementService = entitlementService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var featureAttributes = request.GetType().GetCustomAttributes(typeof(RequiresFeatureAttribute), true)
            .Cast<RequiresFeatureAttribute>()
            .ToList();

        if (featureAttributes.Count == 0)
            return await next();

        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            throw new UnauthorizedAccessException();

        var userId = userIdNullable.Value;
        foreach (var attr in featureAttributes)
        {
            var hasFeature = await _entitlementService.HasFeatureAsync(userId, attr.FeatureKey, cancellationToken);
            if (!hasFeature)
                throw new ForbiddenAccessException($"Feature '{attr.FeatureKey}' is not available for current package.");
        }

        return await next();
    }
}

