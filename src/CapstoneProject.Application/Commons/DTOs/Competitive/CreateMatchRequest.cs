namespace CapstoneProject.Application.Commons.DTOs.Competitive;

public class CreateMatchRequest
{
    public Guid MapId { get; set; }
    public string? RulesSpec { get; set; }
}
