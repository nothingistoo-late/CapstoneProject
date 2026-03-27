using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Xp.Commands.UpdateXpSourceConfig;

public class UpdateXpSourceConfigCommandHandler : IRequestHandler<UpdateXpSourceConfigCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateXpSourceConfigCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateXpSourceConfigCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("Only Admin/Moderator can update XP source configs.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<XpSourceConfig>();
        var config = await repo.GetQueryable().FirstOrDefaultAsync(x => x.SourceType == request.SourceType && !x.IsDeleted, cancellationToken);
        if (config == null)
            return Result.Failure($"Source config not found for source: {request.SourceType}.", ErrorCodeEnum.NotFound);

        config.IsEnabled = request.IsEnabled;
        config.BaseXp = request.BaseXp;
        config.DailyCap = request.DailyCap;
        config.BonusMultiplier = request.BonusMultiplier;
        config.ConfigJson = request.ConfigJson;
        config.UpdateEntity(userId);
        repo.Update(config);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("XP source config updated.");
    }
}

