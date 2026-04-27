using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Commands.AddMapToMyGames;

public class AddMapToMyGamesCommandHandler : IRequestHandler<AddMapToMyGamesCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddMapToMyGamesCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(AddMapToMyGamesCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thêm trò chơi vào bộ sưu tập của bạn.", ErrorCodeEnum.Unauthorized);

        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.GameId && !m.IsDeleted && m.Status == EntityStatusEnum.Active, cancellationToken);
        if (game == null)
            return Result.Failure("Trò chơi không được tìm thấy hoặc không hoạt động.", ErrorCodeEnum.NotFound);

        if (!(game.Price == null || game.Price <= 0))
            return Result.Failure("Chỉ có thể thêm trò chơi miễn phí vào bộ sưu tập của bạn. Trò chơi này được trả tiền.", ErrorCodeEnum.InvalidOperation);

        if (!game.IsPublished || game.GameStatus != GameStatusEnum.Published)
            return Result.Failure("Chỉ những trò chơi miễn phí đã xuất bản mới có thể được thêm vào bộ sưu tập của bạn.", ErrorCodeEnum.InvalidOperation);

        var myMapRepo = _unitOfWork.Repository<MyGame>();
        var alreadyExists = await myMapRepo.GetQueryable()
            .AnyAsync(mm => !mm.IsDeleted && mm.UserId == userId.Value && mm.GameId == request.GameId, cancellationToken);
        if (alreadyExists)
            return Result.Success("Trò chơi đã có trong bộ sưu tập của bạn.");

        var myMap = new MyGame { GameId = request.GameId, UserId = userId.Value, IsAuthor = false };
        myMap.InitializeEntity(userId);
        await myMapRepo.AddAsync(myMap);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Trò chơi miễn phí được thêm vào bộ sưu tập của bạn.");
    }
}
