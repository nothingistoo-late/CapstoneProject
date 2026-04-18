using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.DeleteTag;

public record DeleteTagCommand(Guid TagId) : IRequest<Result>;
