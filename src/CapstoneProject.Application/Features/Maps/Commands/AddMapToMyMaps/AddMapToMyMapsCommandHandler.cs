using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.AddMapToMyMaps;

public class AddMapToMyMapsCommandHandler : IRequestHandler<AddMapToMyMapsCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddMapToMyMapsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(AddMapToMyMapsCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thêm bản đồ vào bộ sưu tập của bạn.", ErrorCodeEnum.Unauthorized);

        var map = await _unitOfWork.Repository<Map>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MapId && !m.IsDeleted && m.Status == EntityStatusEnum.Active, cancellationToken);
        if (map == null)
            return Result.Failure("Bản đồ không được tìm thấy hoặc không hoạt động.", ErrorCodeEnum.NotFound);

        if (!(map.Price == null || map.Price <= 0))
            return Result.Failure("Chỉ có thể thêm bản đồ miễn phí vào bộ sưu tập của bạn. Bản đồ này được trả tiền.", ErrorCodeEnum.InvalidOperation);

        if (!map.IsPublished || map.MapStatus != MapStatusEnum.Published)
            return Result.Failure("Chỉ những bản đồ miễn phí đã xuất bản mới có thể được thêm vào bộ sưu tập của bạn.", ErrorCodeEnum.InvalidOperation);

        var myMapRepo = _unitOfWork.Repository<MyMap>();
        var alreadyExists = await myMapRepo.GetQueryable()
            .AnyAsync(mm => !mm.IsDeleted && mm.UserId == userId.Value && mm.MapId == request.MapId, cancellationToken);
        if (alreadyExists)
            return Result.Success("Bản đồ đã có trong bộ sưu tập của bạn.");

        var myMap = new MyMap { MapId = request.MapId, UserId = userId.Value, IsAuthor = false };
        myMap.InitializeEntity(userId);
        await myMapRepo.AddAsync(myMap);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Bản đồ miễn phí được thêm vào bộ sưu tập của bạn.");
    }
}
