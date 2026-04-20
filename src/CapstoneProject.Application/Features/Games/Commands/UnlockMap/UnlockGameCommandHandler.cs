using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Games.Commands.UnlockMap;

public class UnlockMapCommandHandler : IRequestHandler<UnlockMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UnlockMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UnlockMapCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu c?u xác th?c. Vui lòng ??ng nh?p ?? m? khóa game.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("B?n không có quy?n m? khóa game. Ch? Qu?n tr? viên ho?c Ng??i ?i?u hành m?i có th? th?c hi?n.", ErrorCodeEnum.Forbidden);

        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == request.GameId && !m.IsDeleted, cancellationToken);
        if (game == null)
            return Result.Failure($"Không tìm th?y game có Id: {request.GameId}.", ErrorCodeEnum.NotFound);

        if (game.Status == EntityStatusEnum.Active)
            return Result.Success("Game ?ã ? tr?ng thái ho?t ??ng.");

        game.Status = EntityStatusEnum.Active;

        if (request.RepublishIfPublishedStatus && game.GameStatus == GameStatusEnum.Published)
            game.IsPublished = true;

        game.UpdateEntity(userIdNullable.Value);
        _unitOfWork.Repository<Game>().Update(game);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("?ã m? khóa game thành công.");
    }
}
