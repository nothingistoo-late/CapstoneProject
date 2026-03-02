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
