using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Xp.Queries.GetMyXpProfile;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Queries.GetUserXpProfile;

public record GetUserXpProfileQuery(Guid UserId) : IRequest<Result<MyXpProfileDto>>;

