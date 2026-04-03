using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.DuplicateMapAsNew;

public record DuplicateMapAsNewCommand(Guid SourceMapId, DuplicateMapAsNewRequest? Request = null)
    : IRequest<Result<Guid>>;
