using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Models.Xp;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Commands.GrantXpToUser;

public class GrantXpToUserCommandHandler : IRequestHandler<GrantXpToUserCommand, Result<XpGrantResult>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IXpEngineService _xpEngineService;

    public GrantXpToUserCommandHandler(ICurrentUserService currentUserService, IXpEngineService xpEngineService)
    {
        _currentUserService = currentUserService;
        _xpEngineService = xpEngineService;
    }

    public async Task<Result<XpGrantResult>> Handle(GrantXpToUserCommand request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<XpGrantResult>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<XpGrantResult>.Failure("Chỉ Quản trị viên/Người điều hành mới có thể cấp XP.", ErrorCodeEnum.Forbidden);

        var input = new XpGrantInput
        {
            UserId = request.UserId,
            RequestedXp = request.Amount,
            SourceType = request.SourceType,
            SourceId = request.SourceId,
            IdempotencyKey = request.IdempotencyKey,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? "Admin grant" : request.Reason,
            Metadata = request.Metadata
        };
        return await _xpEngineService.GrantXpAsync(input, cancellationToken);
    }
}

