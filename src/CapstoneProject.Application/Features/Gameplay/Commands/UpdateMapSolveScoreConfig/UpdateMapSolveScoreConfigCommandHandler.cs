using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Gameplay.Commands.UpdateMapSolveScoreConfig;

public class UpdateMapSolveScoreConfigCommandHandler : IRequestHandler<UpdateMapSolveScoreConfigCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMapSolveScoreConfigCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateMapSolveScoreConfigCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("Only Admin/Moderator can update map solve score config.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<MapSolveScoreConfig>();
        var row = await repo.GetQueryable()
            .FirstOrDefaultAsync(x => x.ConfigKey == MapSolveScoreConfig.DefaultConfigKey, cancellationToken);
        if (row == null)
            return Result.Failure("Map solve score config not found.", ErrorCodeEnum.NotFound);

        row.BaseScore = request.BaseScore;
        row.TimeScore = request.TimeScore;
        row.StepsScore = request.StepsScore;
        row.BlocksScore = request.BlocksScore;
        row.UpdateEntity(userId);
        repo.Update(row);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Map solve score config updated.");
    }
}
