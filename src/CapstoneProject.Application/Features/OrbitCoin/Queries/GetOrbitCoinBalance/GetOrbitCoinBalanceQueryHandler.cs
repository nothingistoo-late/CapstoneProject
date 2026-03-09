using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using CapstoneProject.Application.Commons.Interfaces;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetOrbitCoinBalance;

public class GetOrbitCoinBalanceQueryHandler : IRequestHandler<GetOrbitCoinBalanceQuery, Result<OrbitCoinBalanceDto>>
{
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly ICurrentUserService _currentUserService;

    public GetOrbitCoinBalanceQueryHandler(IOrbitCoinService orbitCoinService, ICurrentUserService currentUserService)
    {
        _orbitCoinService = orbitCoinService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<OrbitCoinBalanceDto>> Handle(GetOrbitCoinBalanceQuery request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        if (!userId.HasValue)
        {
            var (isValid, id) = await _currentUserService.IsUserValidAsync();
            if (!isValid || !id.HasValue)
                return Result<OrbitCoinBalanceDto>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);
            userId = id.Value;
        }
        var balance = await _orbitCoinService.GetBalanceAsync(userId.Value, cancellationToken);
        return Result<OrbitCoinBalanceDto>.Success(new OrbitCoinBalanceDto { Balance = balance }, "Balance retrieved.");
    }
}
