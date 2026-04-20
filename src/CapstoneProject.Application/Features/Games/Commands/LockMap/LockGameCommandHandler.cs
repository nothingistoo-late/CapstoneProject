using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CapstoneProject.Application.Features.Games.Commands.LockMap;

public class LockMapCommandHandler : IRequestHandler<LockMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public LockMapCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result> Handle(LockMapCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu c?u xác th?c. Vui lòng ??ng nh?p ?? khóa game.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("B?n không có quy?n khóa game. Ch? Qu?n tr? viên ho?c Ng??i ?i?u hành m?i có th? th?c hi?n.", ErrorCodeEnum.Forbidden);

        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == request.GameId && !m.IsDeleted, cancellationToken);
        if (game == null)
            return Result.Failure($"Không tìm th?y game có Id: {request.GameId}.", ErrorCodeEnum.NotFound);

        if (game.Status == EntityStatusEnum.Inactive)
            return Result.Success("Game ?ã ? tr?ng thái b? khóa.");

        game.Status = EntityStatusEnum.Inactive;
        game.IsPublished = false;
        if (!string.IsNullOrWhiteSpace(request.Note))
            game.ReviewNote = request.Note.Trim();

        game.UpdateEntity(userIdNullable.Value);
        _unitOfWork.Repository<Game>().Update(game);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("?ã khóa game thành công. Game s? không hi?n th? trong catalog learner.");
    }
}
