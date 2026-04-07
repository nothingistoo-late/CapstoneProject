using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Competitive.Commands.CreateMatch;

public class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateMatchCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateMatchCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để tạo trận đấu.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var mapExists = await _unitOfWork.Repository<Map>().GetQueryable().AnyAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (!mapExists)
            return Result<Guid>.Failure("Không tìm thấy bản đồ", ErrorCodeEnum.NotFound);

        var match = new Match
        {
            MapId = command.MapId,
            RulesSpec = command.RulesSpec
        };
        match.InitializeEntity(userId);
        await _unitOfWork.Repository<Match>().AddAsync(match);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(match.Id, "Đã tạo trận đấu.");
    }
}
