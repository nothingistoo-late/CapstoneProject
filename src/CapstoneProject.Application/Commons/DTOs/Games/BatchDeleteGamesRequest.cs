namespace CapstoneProject.Application.Commons.DTOs.Games;

public class BatchDeleteMapsRequest
{
    public List<Guid> Ids { get; set; } = new();
}
