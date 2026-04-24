using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetCmsOrbitCoinInsights;

public record GetCmsOrbitCoinInsightsQuery(int Top = 5) : IRequest<Result<CmsOrbitCoinInsightsDto>>;

public class CmsOrbitCoinInsightsDto
{
    public decimal IssuedOc { get; set; }
    public decimal ConsumedOc { get; set; }
    public decimal CirculatingOc { get; set; }
    public List<CmsOrbitCoinHolderDto> TopHolders { get; set; } = [];
}

public class CmsOrbitCoinHolderDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public decimal BalanceOc { get; set; }
}

public class GetCmsOrbitCoinInsightsQueryHandler : IRequestHandler<GetCmsOrbitCoinInsightsQuery, Result<CmsOrbitCoinInsightsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCmsOrbitCoinInsightsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CmsOrbitCoinInsightsDto>> Handle(GetCmsOrbitCoinInsightsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<CmsOrbitCoinInsightsDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin))
            return Result<CmsOrbitCoinInsightsDto>.Failure("Chỉ quản trị viên mới có thể truy cập.", ErrorCodeEnum.Forbidden);

        var top = Math.Clamp(request.Top, 1, 20);

        var paymentRows = _unitOfWork.Repository<PaymentRecord>()
            .GetQueryable()
            .AsNoTracking()
            .Where(pr => !pr.IsDeleted && pr.PaymentStatus == PaymentStatusEnum.Completed);

        var issuedOc = await paymentRows
            .Where(pr => pr.GameId == null && pr.PackageId == null)
            .SumAsync(pr => (decimal?)pr.Amount, cancellationToken) ?? 0m;

        var consumedOc = await paymentRows
            .Where(pr => pr.GameId != null || pr.PackageId != null)
            .SumAsync(pr => (decimal?)pr.Amount, cancellationToken) ?? 0m;

        var wallets = await _unitOfWork.Repository<UserWallet>()
            .GetQueryable()
            .AsNoTracking()
            .Where(w => !w.IsDeleted && w.Balance > 0)
            .OrderByDescending(w => w.Balance)
            .Take(top)
            .ToListAsync(cancellationToken);

        var walletUserIds = wallets.Select(w => w.UserId).Distinct().ToList();
        var users = await _unitOfWork.Repository<AppUser>()
            .GetQueryable()
            .AsNoTracking()
            .Where(u => walletUserIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var topHolders = wallets.Select(w =>
        {
            users.TryGetValue(w.UserId, out var user);
            var fullName = user == null ? w.UserId.ToString() : $"{user.FirstName} {user.LastName}".Trim();
            return new CmsOrbitCoinHolderDto
            {
                UserId = w.UserId,
                UserName = string.IsNullOrWhiteSpace(fullName) ? w.UserId.ToString() : fullName,
                UserEmail = user?.Email ?? string.Empty,
                BalanceOc = w.Balance
            };
        }).ToList();

        // Circulating OC in this dashboard is defined as net issued amount
        // (issued through top-up flows minus OC consumed by purchases).
        // This avoids inflated values caused by historical/test wallets.
        var circulatingOc = Math.Max(0m, issuedOc - consumedOc);

        var dto = new CmsOrbitCoinInsightsDto
        {
            IssuedOc = issuedOc,
            ConsumedOc = consumedOc,
            CirculatingOc = circulatingOc,
            TopHolders = topHolders
        };

        return Result<CmsOrbitCoinInsightsDto>.Success(dto, "Đã lấy tổng quan OrbitCoin cho CMS.");
    }
}
