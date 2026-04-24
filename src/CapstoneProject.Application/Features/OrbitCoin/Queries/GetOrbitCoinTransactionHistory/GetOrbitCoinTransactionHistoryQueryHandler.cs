using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetOrbitCoinTransactionHistory;

public class GetOrbitCoinTransactionHistoryQueryHandler : IRequestHandler<GetOrbitCoinTransactionHistoryQuery, Result<OrbitCoinTransactionHistoryResult>>
{
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly ICurrentUserService _currentUserService;

    public GetOrbitCoinTransactionHistoryQueryHandler(IOrbitCoinService orbitCoinService, ICurrentUserService currentUserService)
    {
        _orbitCoinService = orbitCoinService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<OrbitCoinTransactionHistoryResult>> Handle(GetOrbitCoinTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        if (!userId.HasValue)
        {
            var (isValid, id) = await _currentUserService.IsUserValidAsync();
            if (!isValid || !id.HasValue)
                return Result<OrbitCoinTransactionHistoryResult>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);
            userId = id.Value;
        }
        var (items, total) = await _orbitCoinService.GetTransactionHistoryAsync(
            userId.Value,
            request.PageNumber,
            request.PageSize,
            new OrbitCoinTransactionFilter
            {
                Direction = request.Direction,
                Categories = request.Categories,
                RelatedEntityType = request.RelatedEntityType,
                RelatedEntityId = request.RelatedEntityId,
                From = request.From,
                To = request.To,
                MinAmount = request.MinAmount,
                MaxAmount = request.MaxAmount,
                Status = request.Status,
                Statuses = request.Statuses,
                Search = request.Search
            },
            cancellationToken);
        var result = new OrbitCoinTransactionHistoryResult
        {
            Items = items,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            AvailableStatuses = Enum.GetNames<PaymentStatusEnum>()
        };
        return Result<OrbitCoinTransactionHistoryResult>.Success(result, "Lịch sử giao dịch được truy xuất.");
    }
}
