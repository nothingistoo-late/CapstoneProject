using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Gameplay.Queries.GetMapSolveScoreConfig;

public class GetMapSolveScoreConfigQueryHandler : IRequestHandler<GetMapSolveScoreConfigQuery, Result<MapSolveScoreConfigDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMapSolveScoreConfigQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MapSolveScoreConfigDto>> Handle(GetMapSolveScoreConfigQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<MapSolveScoreConfigDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<MapSolveScoreConfigDto>.Failure("Chỉ Quản trị viên/Người điều hành mới có thể xem cấu hình điểm giải quyết bản đồ.", ErrorCodeEnum.Forbidden);

        var row = await _unitOfWork.Repository<MapSolveScoreConfig>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ConfigKey == MapSolveScoreConfig.DefaultConfigKey, cancellationToken);

        if (row == null)
            return Result<MapSolveScoreConfigDto>.Failure("Không tìm thấy cấu hình điểm giải quyết bản đồ.", ErrorCodeEnum.NotFound);

        return Result<MapSolveScoreConfigDto>.Success(new MapSolveScoreConfigDto
        {
            ConfigKey = row.ConfigKey,
            BaseScore = row.BaseScore,
            TimeScore = row.TimeScore,
            StepsScore = row.StepsScore,
            BlocksScore = row.BlocksScore
        }, "Đã lấy cấu hình điểm giải bản đồ.");
    }
}

