using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Marketplace.Commands.DeletePackage;

public class DeletePackageCommandHandler : IRequestHandler<DeletePackageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeletePackageCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeletePackageCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to delete a package.", ErrorCodeEnum.Unauthorized);
        if (!(await _currentUserService.GetCurrentRolesAsync()).Contains(RoleEnum.Admin))
            return Result.Failure("You do not have permission to delete packages. Only Admin can perform this action.", ErrorCodeEnum.Forbidden);

        var pkg = await _unitOfWork.Repository<Package>().GetQueryable().FirstOrDefaultAsync(p => p.Id == command.PackageId && !p.IsDeleted, cancellationToken);
        if (pkg == null)
            return Result.Failure($"Package not found with Id: {command.PackageId}. The package may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        pkg.SoftDeleteEntity(userIdNullable.Value);
        _unitOfWork.Repository<Package>().Update(pkg);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Package deleted.");
    }
}
