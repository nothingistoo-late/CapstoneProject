using MediatR;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Auth.Queries.GetProfile;

public record GetProfileQuery : IRequest<Result<ProfileResponse>>;