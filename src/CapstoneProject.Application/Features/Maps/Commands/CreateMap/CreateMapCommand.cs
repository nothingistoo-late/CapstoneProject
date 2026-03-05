using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMap;

public record CreateMapCommand(CreateMapRequest Request, bool AutoPublish = false) : IRequest<Result<Guid>>;
