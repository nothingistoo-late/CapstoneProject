using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.UpdateMapFromJsonFile;

public record UpdateMapFromJsonFileCommand(Guid MapId, CreateMapFromJsonFileInput Input) : IRequest<Result>;

