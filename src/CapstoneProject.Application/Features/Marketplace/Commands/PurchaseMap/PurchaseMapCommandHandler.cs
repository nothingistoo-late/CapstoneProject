using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Marketplace.Commands.PurchaseMap;

public class PurchaseMapCommandHandler : IRequestHandler<PurchaseMapCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PurchaseMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(PurchaseMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Authentication required. Please log in to purchase a map.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var map = await _unitOfWork.Repository<Map>().GetQueryable().FirstOrDefaultAsync(m => m.Id == command.MapId && !m.IsDeleted, cancellationToken);
        if (map == null)
            return Result<Guid>.Failure($"Map not found with Id: {command.MapId}. The map may have been deleted or does not exist.", ErrorCodeEnum.NotFound);
        if (map.Price == null || map.Price <= 0)
            return Result<Guid>.Failure("This map is free and does not require a purchase. You can access it directly.", ErrorCodeEnum.InvalidOperation);

        var record = new PaymentRecord
        {
            UserId = userId,
            MapId = map.Id,
            Amount = map.Price.Value,
            PaymentStatus = PaymentStatusEnum.Completed,
            PaidAt = DateTime.UtcNow,
            PaymentId = command.PaymentMethodId
        };
        record.InitializeEntity(userId);
        await _unitOfWork.Repository<PaymentRecord>().AddAsync(record);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(record.Id, "Map purchased.");
    }
}
