using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.User.Commands.BatchUpdateUserStatus;

public class BatchUpdateUserStatusCommandHandler : IRequestHandler<BatchUpdateUserStatusCommand, Result<BatchUpdateUserStatusResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BatchUpdateUserStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BatchUpdateUserStatusResultDto>> Handle(BatchUpdateUserStatusCommand command, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<BatchUpdateUserStatusResultDto>.Failure("Authentication required. Please log in to update user status.", ErrorCodeEnum.Unauthorized);
        if (!(await _currentUserService.GetCurrentRolesAsync()).Contains(RoleEnum.Admin))
            return Result<BatchUpdateUserStatusResultDto>.Failure("You do not have permission to update user status. Only Admin can perform this action.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<AppUser>();
        var users = await repo.GetQueryable()
            .Where(u => command.UserIds.Contains(u.Id))
            .ToListAsync(cancellationToken);
        var foundIds = users.Select(u => u.Id).ToHashSet();
        var notFoundIds = command.UserIds.Where(id => !foundIds.Contains(id)).ToList();

        foreach (var user in users)
        {
            user.Status = command.Status;
            repo.Update(user);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BatchUpdateUserStatusResultDto
        {
            SuccessCount = users.Count,
            FailedCount = notFoundIds.Count,
            NotFoundIds = notFoundIds
        };
        return Result<BatchUpdateUserStatusResultDto>.Success(dto, $"Updated status for {dto.SuccessCount} user(s).");
    }
}
