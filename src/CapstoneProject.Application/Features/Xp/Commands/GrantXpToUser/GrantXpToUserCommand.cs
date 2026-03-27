using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Models.Xp;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Commands.GrantXpToUser;

public record GrantXpToUserCommand(
    Guid UserId,
    int Amount,
    XpSourceTypeEnum SourceType,
    Guid? SourceId,
    string IdempotencyKey,
    string Reason,
    string? Metadata) : IRequest<Result<XpGrantResult>>;

