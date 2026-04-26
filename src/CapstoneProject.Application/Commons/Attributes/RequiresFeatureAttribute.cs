namespace CapstoneProject.Application.Common.Attributes;

/// <summary>
/// Requires a package feature to execute a CQRS request.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequiresFeatureAttribute : Attribute
{
    public RequiresFeatureAttribute(string featureKey)
    {
        FeatureKey = featureKey;
    }

    public string FeatureKey { get; }
}

