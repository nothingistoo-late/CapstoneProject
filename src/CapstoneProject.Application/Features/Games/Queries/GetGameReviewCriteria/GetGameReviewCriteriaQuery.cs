using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Queries.GetGameReviewCriteria;

public record GetGameReviewCriteriaQuery : IRequest<Result<List<GameReviewCriterionDto>>>;
