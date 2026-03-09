using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMapFromJsonFile;

public record CreateMapFromJsonFileCommand(CreateMapFromJsonFileInput Input, bool AutoPublish = false) : IRequest<Result<Guid>>;
