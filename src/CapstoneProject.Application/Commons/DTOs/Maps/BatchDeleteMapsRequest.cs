namespace CapstoneProject.Application.Commons.DTOs.Maps;

public class BatchDeleteMapsRequest
{
    public List<Guid> Ids { get; set; } = new();
}
