using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.Application.Features.User.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteUserCommandHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid)
            {
                throw new UnauthorizedAccessException("Người dùng chưa được xác thực");
            }

            var user = await _identityService.GetUserByIdAsync(command.UserId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException("Không tìm thấy người dùng");
            }

            // Prevent self-deletion
            if (_currentUserService.UserId == command.UserId.ToString())
            {
                throw new ArgumentException("Cannot delete your own account");
            }

            // Soft delete using entity extension
            user.DeactivateUser(userId);

            var updateResult = await _identityService.UpdateUserAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                return Result.Failure("Không thể xóa người dùng", ErrorCodeEnum.ValidationFailed, errors);
            }

            return Result.Success("Người dùng đã xóa thành công");
        }
        catch
        {
            throw;
        }
    }
}
