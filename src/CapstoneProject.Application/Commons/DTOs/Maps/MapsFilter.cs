using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class MapsFilter : BasePaginationFilter
{
    public string? ExternalId { get; set; }
}
