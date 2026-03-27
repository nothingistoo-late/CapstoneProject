using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Queries.GetMyXpProfile;

public record GetMyXpProfileQuery() : IRequest<Result<MyXpProfileDto>>;

public class MyXpProfileDto
{
    public Guid UserId { get; set; }
    public int CurrentXp { get; set; }
    public int CurrentLevel { get; set; }
    public int NextLevel { get; set; }
    public int XpToNextLevel { get; set; }
    public double ProgressPercent { get; set; }
}

