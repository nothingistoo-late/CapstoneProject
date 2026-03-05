using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.DeleteTag;

public record DeleteTagCommand(Guid TagId) : IRequest<Result>;
