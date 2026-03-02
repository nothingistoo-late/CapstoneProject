using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Marketplace.Commands.UpdatePackage;

public class UpdatePackageCommandHandler : IRequestHandler<UpdatePackageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePackageCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdatePackageCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to update a package.", ErrorCodeEnum.Unauthorized);
        if (!(await _currentUserService.GetCurrentRolesAsync()).Contains(RoleEnum.Admin))
            return Result.Failure("You do not have permission to update packages. Only Admin can perform this action.", ErrorCodeEnum.Forbidden);

        var pkg = await _unitOfWork.Repository<Package>().GetQueryable().FirstOrDefaultAsync(p => p.Id == command.PackageId && !p.IsDeleted, cancellationToken);
        if (pkg == null)
            return Result.Failure($"Package not found with Id: {command.PackageId}. The package may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        var req = command.Request;
        if (req.Name != null) pkg.Name = req.Name;
        if (req.DurationDays.HasValue) pkg.DurationDays = req.DurationDays.Value;
        if (req.Limit.HasValue) pkg.Limit = req.Limit;
        if (req.Price.HasValue) pkg.Price = req.Price.Value;
        if (req.FeaturesSpec != null) pkg.FeaturesSpec = req.FeaturesSpec;
        if (req.IsActive.HasValue) pkg.Status = req.IsActive.Value ? EntityStatusEnum.Active : EntityStatusEnum.Inactive;
        pkg.UpdateEntity(userIdNullable!.Value);
        _unitOfWork.Repository<Package>().Update(pkg);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Package updated.");
    }
}
