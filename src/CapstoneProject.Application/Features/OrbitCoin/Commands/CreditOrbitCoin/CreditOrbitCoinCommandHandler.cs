using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.CreditOrbitCoin;

public class CreditOrbitCoinCommandHandler : IRequestHandler<CreditOrbitCoinCommand, Result>
{
    private readonly IOrbitCoinService _orbitCoinService;
    private readonly ICurrentUserService _currentUserService;

    public CreditOrbitCoinCommandHandler(IOrbitCoinService orbitCoinService, ICurrentUserService currentUserService)
    {
        _orbitCoinService = orbitCoinService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CreditOrbitCoinCommand request, CancellationToken cancellationToken)
    {
        var (isValid, adminId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !adminId.HasValue)
            return Result.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        if (request.Amount <= 0)
            return Result.Failure("Số tiền phải dương.", ErrorCodeEnum.ValidationFailed);

        var (success, error) = await _orbitCoinService.CreditAsync(
            request.UserId,
            request.Amount,
            CoinTransactionTypeEnum.EarnDeposit,
            request.RelatedEntityType,
            request.RelatedEntityId,
            feeAmount: 0,
            request.Note,
            adminId,
            cancellationToken);

        if (!success)
            return Result.Failure(error ?? "Tín dụng không thành công.", ErrorCodeEnum.InvalidOperation);
        return Result.Success("OrbitCoin đã ghi có (tiền gửi được ghi lại).");
    }
}
