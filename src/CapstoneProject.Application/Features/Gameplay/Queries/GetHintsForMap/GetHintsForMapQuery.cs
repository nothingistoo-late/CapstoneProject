using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetHintsForMap;

public record GetHintsForMapQuery(Guid GameId, Guid? GameDetailId = null) : IRequest<Result<List<HintLevelDto>>>;

public class HintLevelDto
{
    public int LevelOrder { get; set; }
    public Guid GameDetailId { get; set; }
    public int OrderNo { get; set; }
    public string Content { get; set; } = string.Empty;
}
