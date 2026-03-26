using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Chat.Commands.DeleteMessage;

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteMessageCommandHandler> _logger;

    public DeleteMessageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteMessageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteMessageCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Result<bool>.Failure("User not authenticated", ErrorCodeEnum.Unauthorized);
            }

            if (command == null)
            {
                return Result<bool>.Failure("Command cannot be null", ErrorCodeEnum.InvalidInput);
            }

            if (command.MessageId == Guid.Empty)
            {
                return Result<bool>.Failure("Message ID is required", ErrorCodeEnum.InvalidInput);
            }

            var messageRepo = _unitOfWork.Repository<Message>();

            var message = await messageRepo.GetQueryable()
                .Where(m => m.Id == command.MessageId && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (message == null)
            {
                return Result<bool>.Failure("Message not found or already deleted", ErrorCodeEnum.NotFound);
            }

            if (message.SenderId != currentUserId)
            {
                return Result<bool>.Failure("You can only delete your own messages", ErrorCodeEnum.Forbidden);
            }

            message.IsDeleted = true;
            message.DeletedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
            message.DeletedBy = currentUserId;
            message.UpdatedBy = currentUserId;
            message.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;

            messageRepo.Update(message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} deleted message {MessageId}", currentUserId, command.MessageId);

            return Result<bool>.Success(true);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting message {MessageId}", command?.MessageId);
            return Result<bool>.Failure("Failed to delete message due to database error", ErrorCodeEnum.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting message {MessageId}", command?.MessageId);
            return Result<bool>.Failure("An unexpected error occurred while deleting the message", ErrorCodeEnum.InternalError);
        }
    }
}

