using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Commands.DeleteMap;

public class DeleteMapCommandHandler : IRequestHandler<DeleteMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xóa bản đồ.", ErrorCodeEnum.Unauthorized);

        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .FirstOrDefaultAsync(m => m.Id == command.GameId && !m.IsDeleted, cancellationToken);
        if (game == null)
            return Result.Failure($"Không tìm thấy bản đồ có Id: {command.GameId}. Bản đồ có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        if (game.CreatedBy != userIdNullable.Value && !isAdminOrMod)
            return Result.Failure("Bạn không có quyền xóa bản đồ này. Chỉ tác giả bản đồ hoặc Quản trị viên/Người điều hành mới có thể xóa nó.", ErrorCodeEnum.Forbidden);

        game.IsPublished = false;
        if (game.GameStatus == GameStatusEnum.Published)
            game.GameStatus = GameStatusEnum.Approved;

        game.SoftDeleteEntity(userIdNullable.Value);
        _unitOfWork.Repository<Game>().Update(game);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Bản đồ đã được xóa thành công.");
    }
}
