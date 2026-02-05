using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Chat.Commands.AddMemberToRoom;

public class AddMemberToRoomCommandValidator : AbstractValidator<AddMemberToRoomCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddMemberToRoomCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        RuleFor(x => x.ChatRoomId)
            .NotEmpty().WithMessage("Chat room ID is required")
            .MustAsync(async (chatRoomId, cancellation) =>
            {
                if (chatRoomId == Guid.Empty)
                    return false;

                var conversationRepo = _unitOfWork.Repository<Domain.Entities.ChatRoom>();
                var conversation = await conversationRepo.GetFirstOrDefaultAsync(
                    c => c.Id == chatRoomId && !c.IsDeleted);

                return conversation != null;
            }).WithMessage("Conversation not found");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .MustAsync(async (userId, cancellation) =>
            {
                if (userId == Guid.Empty)
                    return false;

                var userRepo = _unitOfWork.Repository<Domain.Entities.AppUser>();
                var user = await userRepo.GetFirstOrDefaultAsync(
                    u => u.Id == userId && u.Status == EntityStatusEnum.Active);

                return user != null;
            }).WithMessage("User does not exist or is not active");

        RuleFor(x => x.ChatRoomId)
            .MustAsync(async (chatRoomId, cancellation) =>
            {
                if (chatRoomId == Guid.Empty)
                    return false;

                var conversationRepo = _unitOfWork.Repository<Domain.Entities.ChatRoom>();
                var conversation = await conversationRepo.GetFirstOrDefaultAsync(
                    c => c.Id == chatRoomId && !c.IsDeleted);

                if (conversation == null)
                    return false;

                // Cannot add members to closed conversations
                return !conversation.IsClosed;
            }).WithMessage("Cannot add members to a closed conversation");

        RuleFor(x => x.ChatRoomId)
            .MustAsync(async (chatRoomId, cancellation) =>
            {
                if (chatRoomId == Guid.Empty)
                    return false;

                var conversationRepo = _unitOfWork.Repository<Domain.Entities.ChatRoom>();
                var conversation = await conversationRepo.GetFirstOrDefaultAsync(
                    c => c.Id == chatRoomId && !c.IsDeleted);

                if (conversation == null)
                    return false;

                // Only temporary group conversations allow adding members dynamically
                return conversation.RoomType == ChatRoomTypeEnum.TemporaryGroup;
            }).WithMessage("Only temporary group conversations allow adding members");

        // Cannot add to private conversations (they always have exactly 2 members)
        RuleFor(x => x)
            .MustAsync(async (command, cancellation) =>
            {
                var conversationRepo = _unitOfWork.Repository<Domain.Entities.ChatRoom>();
                var conversation = await conversationRepo.GetFirstOrDefaultAsync(
                    c => c.Id == command.ChatRoomId && !c.IsDeleted);

                if (conversation == null || conversation.RoomType != ChatRoomTypeEnum.Private)
                    return true; // Validation passed for non-private conversations

                // Private conversations should not use this endpoint
                return false;
            }).WithMessage("Cannot add members to private conversations. Private conversations are limited to 2 participants.");
    }
}
