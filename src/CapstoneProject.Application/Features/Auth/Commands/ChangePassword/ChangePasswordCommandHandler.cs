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
                return Result.Failure("Not authorized", ErrorCodeEnum.Unauthorized);
            }
            var user = await _identityService.GetUserByIdAsync(userId.Value.ToString());
            user.UpdateEntity(userId);
            var result = await _identityService.ChangePasswordAsync(user, request.Request.CurrentPassword, request.Request.NewPassword);
            if (!result.Succeeded && result.Errors.Any(x => x.Code == "PasswordMismatch"))
            {
                return Result.Failure("Current password is incorrect", ErrorCodeEnum.Unauthorized);
            }
            if (!result.Succeeded)
            {
                return Result.Failure(result.Errors.Select(x => x.Description + " ").ToString() ?? "Error changing password", ErrorCodeEnum.InternalError);
            }
            return Result.Success("Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return Result.Failure("Error changing password", ErrorCodeEnum.InternalError);
        }
    }
}