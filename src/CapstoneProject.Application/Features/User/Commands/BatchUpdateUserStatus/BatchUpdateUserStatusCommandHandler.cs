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
            return Result<BatchUpdateUserStatusResultDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để cập nhật trạng thái người dùng.", ErrorCodeEnum.Unauthorized);
        if (!(await _currentUserService.GetCurrentRolesAsync()).Contains(RoleEnum.Admin))
            return Result<BatchUpdateUserStatusResultDto>.Failure("Bạn không có quyền cập nhật trạng thái người dùng. Chỉ Quản trị viên mới có thể thực hiện hành động này.", ErrorCodeEnum.Forbidden);

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
        return Result<BatchUpdateUserStatusResultDto>.Success(dto, $"Đã cập nhật trạng thái cho {dto.SuccessCount} người dùng.");
    }
}
