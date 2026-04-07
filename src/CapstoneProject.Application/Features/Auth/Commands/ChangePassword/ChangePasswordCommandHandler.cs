using MediatR;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    public ChangePasswordCommandHandler(IIdentityService identityService, ILogger<ChangePasswordCommandHandler> logger, ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _logger = logger;
        _currentUserService = currentUserService;
    }
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId == null)
            {
                return Result.Failure("Không được ủy quyền", ErrorCodeEnum.Unauthorized);
            }
            var user = await _identityService.GetUserByIdAsync(userId.Value.ToString());
            user.UpdateEntity(userId);
            var result = await _identityService.ChangePasswordAsync(user, request.Request.CurrentPassword, request.Request.NewPassword);
            if (!result.Succeeded && result.Errors.Any(x => x.Code == "PasswordMismatch"))
            {
                return Result.Failure("Mật khẩu hiện tại không chính xác", ErrorCodeEnum.Unauthorized);
            }
            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(x => x.Description + " ").ToString() ?? "Lỗi đổi mật khẩu", ErrorCodeEnum.InternalError);
            }
            return Result.Success("Đã thay đổi mật khẩu thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi đổi mật khẩu");
            return Result.Failure("Lỗi đổi mật khẩu", ErrorCodeEnum.InternalError);
        }
    }
}