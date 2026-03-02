namespace CapstoneProject.Application.Commons.DTOs.Marketplace;

public class PackageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int? Limit { get; set; }
    public decimal Price { get; set; }
    public string? FeaturesSpec { get; set; }
    public bool IsActive => Status == Domain.Enums.EntityStatusEnum.Active;
    public Domain.Enums.EntityStatusEnum Status { get; set; }
}

public class CreatePackageRequest
{
    public string Name { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int? Limit { get; set; }
    public decimal Price { get; set; }
    public string? FeaturesSpec { get; set; }
}

public class UpdatePackageRequest
{
    public string? Name { get; set; }
    public int? DurationDays { get; set; }
    public int? Limit { get; set; }
    public decimal? Price { get; set; }
    public string? FeaturesSpec { get; set; }
    public bool? IsActive { get; set; }
}

public class BatchUpdatePackageStatusRequest
{
    public List<Guid> PackageIds { get; set; } = new();
    public bool IsActive { get; set; }
}
