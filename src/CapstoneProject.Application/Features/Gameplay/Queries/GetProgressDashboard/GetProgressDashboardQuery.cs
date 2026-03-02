using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetProgressDashboard;

public record GetProgressDashboardQuery : IRequest<Result<ProgressDashboardDto>>;
