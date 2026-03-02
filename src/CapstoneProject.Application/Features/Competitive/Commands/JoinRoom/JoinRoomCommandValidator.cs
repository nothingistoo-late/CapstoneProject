using FluentValidation;

namespace CapstoneProject.Application.Features.Competitive.Commands.JoinRoom;

public class JoinRoomCommandValidator : AbstractValidator<JoinRoomCommand>
{
    public JoinRoomCommandValidator()
    {
        RuleFor(x => x.RoomCode)
            .NotEmpty().WithMessage("Room code is required.")
            .MaximumLength(20).WithMessage("Room code must not exceed 20 characters.");
    }
}
