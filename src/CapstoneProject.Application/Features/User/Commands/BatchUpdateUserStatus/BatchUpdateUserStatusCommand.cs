using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.User.Commands.BatchUpdateUserStatus;

public record BatchUpdateUserStatusCommand(List<Guid> UserIds, EntityStatusEnum Status) : IRequest<Result<BatchUpdateUserStatusResultDto>>;

public class BatchUpdateUserStatusResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<Guid> NotFoundIds { get; set; } = new();
}
