using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetHintsForMap;

public record GetHintsForMapQuery(Guid MapId) : IRequest<Result<List<HintLevelDto>>>;

public class HintLevelDto
{
    public int OrderNo { get; set; }
    public string Content { get; set; } = string.Empty;
}
