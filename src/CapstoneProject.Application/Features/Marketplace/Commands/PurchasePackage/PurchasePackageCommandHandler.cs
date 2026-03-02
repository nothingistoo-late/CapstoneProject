using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Marketplace.Commands.PurchasePackage;

public class PurchasePackageCommandHandler : IRequestHandler<PurchasePackageCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PurchasePackageCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(PurchasePackageCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Authentication required. Please log in to purchase a package.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var pkg = await _unitOfWork.Repository<Package>().GetQueryable().FirstOrDefaultAsync(p => p.Id == command.PackageId && !p.IsDeleted && p.Status == EntityStatusEnum.Active, cancellationToken);
        if (pkg == null)
            return Result<Guid>.Failure($"Package not found with Id: {command.PackageId}, or the package is inactive and cannot be purchased.", ErrorCodeEnum.NotFound);

        var record = new PaymentRecord
        {
            UserId = userId,
            PackageId = pkg.Id,
            Amount = pkg.Price,
            PaymentStatus = PaymentStatusEnum.Completed,
            PaidAt = DateTime.UtcNow,
            PaymentId = command.PaymentMethodId
        };
        record.InitializeEntity(userId);
        await _unitOfWork.Repository<PaymentRecord>().AddAsync(record);

        var remaining = pkg.Limit ?? 1;
        var expiresAt = DateTime.UtcNow.AddDays(pkg.DurationDays);
        var userPkg = new UserPackage
        {
            UserId = userId,
            PackageId = pkg.Id,
            Remaining = remaining,
            ExpiresAt = expiresAt
        };
        userPkg.InitializeEntity(userId);
        await _unitOfWork.Repository<UserPackage>().AddAsync(userPkg);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(record.Id, "Purchase completed.");
    }
}
