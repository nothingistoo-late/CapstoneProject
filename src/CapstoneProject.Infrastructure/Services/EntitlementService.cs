using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Infrastructure.Services;

public class EntitlementService : IEntitlementService
{
    private readonly IUnitOfWork _unitOfWork;

    public EntitlementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HasFeatureAsync(Guid userId, string featureKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
            return false;

        var normalizedKey = featureKey.Trim().ToLowerInvariant();
        var activePackages = await GetActiveFeatureSpecsAsync(userId, cancellationToken);

        foreach (var featuresSpec in activePackages)
        {
            if (HasFeature(featuresSpec, normalizedKey))
                return true;
        }

        return false;
    }

    public async Task<decimal?> GetNumericFeatureAsync(Guid userId, string featureKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
            return null;

        var normalizedKey = featureKey.Trim().ToLowerInvariant();
        var activePackages = await GetActiveFeatureSpecsAsync(userId, cancellationToken);
        decimal? bestValue = null;

        foreach (var featuresSpec in activePackages)
        {
            if (!TryGetNumericFeature(featuresSpec, normalizedKey, out var value))
                continue;

            if (!bestValue.HasValue || value > bestValue.Value)
                bestValue = value;
        }

        return bestValue;
    }

    private async Task<List<string?>> GetActiveFeatureSpecsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = VietnamDateTime.DbNow;
        return await _unitOfWork.Repository<UserPackage>().GetQueryable()
            .AsNoTracking()
            .Where(up => up.UserId == userId && !up.IsDeleted)
            .Where(up =>
                (up.ExpiresAt == null || up.ExpiresAt > now) &&
                (up.Package.Limit == null || up.Remaining > 0) &&
                !up.Package.IsDeleted &&
                up.Package.Status == EntityStatusEnum.Active)
            .Select(up => up.Package.FeaturesSpec)
            .ToListAsync(cancellationToken);
    }

    private static bool HasFeature(string? featuresSpec, string normalizedFeatureKey)
    {
        if (string.IsNullOrWhiteSpace(featuresSpec))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(featuresSpec);
            var root = doc.RootElement;

            if (TryGetBoolean(root, normalizedFeatureKey, out var direct))
                return direct;

            if (root.TryGetProperty("features", out var featuresNode) &&
                featuresNode.ValueKind == JsonValueKind.Object &&
                TryGetBoolean(featuresNode, normalizedFeatureKey, out var nested))
            {
                return nested;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool TryGetNumericFeature(string? featuresSpec, string normalizedFeatureKey, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(featuresSpec))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(featuresSpec);
            var root = doc.RootElement;

            if (TryGetDecimal(root, normalizedFeatureKey, out var direct))
            {
                value = direct;
                return true;
            }

            if (root.TryGetProperty("features", out var featuresNode) &&
                featuresNode.ValueKind == JsonValueKind.Object &&
                TryGetDecimal(featuresNode, normalizedFeatureKey, out var nested))
            {
                value = nested;
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool TryGetBoolean(JsonElement node, string key, out bool value)
    {
        foreach (var prop in node.EnumerateObject())
        {
            if (!string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                continue;

            if (prop.Value.ValueKind == JsonValueKind.True)
            {
                value = true;
                return true;
            }

            if (prop.Value.ValueKind == JsonValueKind.False)
            {
                value = false;
                return true;
            }

            if (prop.Value.ValueKind == JsonValueKind.String &&
                bool.TryParse(prop.Value.GetString(), out var parsed))
            {
                value = parsed;
                return true;
            }
        }

        value = false;
        return false;
    }

    private static bool TryGetDecimal(JsonElement node, string key, out decimal value)
    {
        foreach (var prop in node.EnumerateObject())
        {
            if (!string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                continue;

            if (prop.Value.ValueKind == JsonValueKind.Number &&
                prop.Value.TryGetDecimal(out var number))
            {
                value = number;
                return true;
            }

            if (prop.Value.ValueKind == JsonValueKind.String &&
                decimal.TryParse(prop.Value.GetString(), out var parsed))
            {
                value = parsed;
                return true;
            }
        }

        value = 0m;
        return false;
    }
}

