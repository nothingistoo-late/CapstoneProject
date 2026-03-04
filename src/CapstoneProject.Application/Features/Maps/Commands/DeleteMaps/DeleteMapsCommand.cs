using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.DeleteMaps;

public record DeleteMapsCommand(Guid Id) : IRequest<Result>;
