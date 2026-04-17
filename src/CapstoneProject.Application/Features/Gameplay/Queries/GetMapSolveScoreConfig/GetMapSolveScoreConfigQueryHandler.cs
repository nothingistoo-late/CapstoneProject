using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetGameSolveScoreConfig;

public class GetGameSolveScoreConfigQueryHandler : IRequestHandler<GetGameSolveScoreConfigQuery, Result<GameSolveScoreConfigDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetGameSolveScoreConfigQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GameSolveScoreConfigDto>> Handle(GetGameSolveScoreConfigQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<GameSolveScoreConfigDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<GameSolveScoreConfigDto>.Failure("Chỉ Quản trị viên/Người điều hành mới có thể xem cấu hình điểm giải quyết bản đồ.", ErrorCodeEnum.Forbidden);

        var row = await _unitOfWork.Repository<GameSolveScoreConfig>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConfigKey == GameSolveScoreConfig.DefaultConfigKey, cancellationToken);

        if (row == null)
            return Result<GameSolveScoreConfigDto>.Failure("Không tìm thấy cấu hình điểm giải quyết bản đồ.", ErrorCodeEnum.NotFound);

        return Result<GameSolveScoreConfigDto>.Success(new GameSolveScoreConfigDto
        {
            ConfigKey = row.ConfigKey,
            BaseScore = row.BaseScore,
            TimeScore = row.TimeScore,
            StepsScore = row.StepsScore,
            BlocksScore = row.BlocksScore
        }, "Đã lấy cấu hình điểm giải bản đồ.");
    }
}

