using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Community.Queries.GetGameRatings;

public record GetGameRatingsQuery(Guid GameId, bool IsAuthorOnly = false) : IRequest<Result<List<GameRatingDto>>>;

public class GameRatingDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }
    /// <summary>Người hiện tại có phải tác giả của rate này không (UserId == currentUserId).</summary>
    public bool IsAuthor { get; set; }
}

