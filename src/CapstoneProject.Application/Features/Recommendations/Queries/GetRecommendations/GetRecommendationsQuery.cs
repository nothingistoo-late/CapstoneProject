using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Recommendations.DTOs;
using MediatR;

namespace CapstoneProject.Application.Features.Recommendations.Queries.GetRecommendations;

public class GetRecommendationsQuery : IRequest<Result<RecommendationResultDto>>
{
}

