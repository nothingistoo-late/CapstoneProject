using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Gameplay.Commands.UpdateGameSolveScoreConfig;

public class UpdateGameSolveScoreConfigCommandHandler : IRequestHandler<UpdateGameSolveScoreConfigCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateGameSolveScoreConfigCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateGameSolveScoreConfigCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("Chỉ Quản trị viên/Người điều hành mới có thể cập nhật cấu hình điểm giải quyết bản đồ.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<GameSolveScoreConfig>();
        var row = await repo.GetQueryable()
            .FirstOrDefaultAsync(x => x.ConfigKey == GameSolveScoreConfig.DefaultConfigKey, cancellationToken);
        if (row == null)
            return Result.Failure("Không tìm thấy cấu hình điểm giải quyết bản đồ.", ErrorCodeEnum.NotFound);

        row.BaseScore = request.BaseScore;
        row.TimeScore = request.TimeScore;
        row.StepsScore = request.StepsScore;
        row.BlocksScore = request.BlocksScore;
        row.UpdateEntity(userId);
        repo.Update(row);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Đã cập nhật cấu hình điểm giải quyết bản đồ.");
    }
}
