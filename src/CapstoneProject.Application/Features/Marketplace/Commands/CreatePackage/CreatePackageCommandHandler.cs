using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Marketplace.Commands.CreatePackage;

public class CreatePackageCommandHandler : IRequestHandler<CreatePackageCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreatePackageCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreatePackageCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để tạo gói.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(Domain.Enums.RoleEnum.Admin))
            return Result<Guid>.Failure("Bạn không có quyền tạo gói. Chỉ Quản trị viên mới có thể thực hiện hành động này.", ErrorCodeEnum.Forbidden);

        var req = command.Request;
        var pkg = new Package
        {
            Name = req.Name,
            DurationDays = req.DurationDays,
            Limit = req.Limit,
            Price = req.Price,
            FeaturesSpec = req.FeaturesSpec
        };
        pkg.InitializeEntity(userIdNullable.Value);
        await _unitOfWork.Repository<Package>().AddAsync(pkg);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(pkg.Id);
    }
}
