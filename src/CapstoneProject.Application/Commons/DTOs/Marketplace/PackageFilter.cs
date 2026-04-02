namespace CapstoneProject.Application.Commons.DTOs.Marketplace;

public class PackageFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    /// <summary>Filter by active/inactive. Null = all.</summary>
    public bool? IsActive { get; set; }
    /// <summary>Search in package name.</summary>
    public string? Search { get; set; }
}

/// <summary>Query cho GET my-packages (lịch sử gói đã mua).</summary>
public class MyPackagesFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    /// <summary>Chỉ các entitlement còn hiệu lực (remaining &gt; 0, chưa hết hạn). Null/false = toàn bộ lịch sử.</summary>
    public bool? ActiveOnly { get; set; }
}
