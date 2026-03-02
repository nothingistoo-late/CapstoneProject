using FluentValidation;

namespace CapstoneProject.Application.Features.Competitive.Commands.CreateRoom;

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithMessage("Match Id is required.");
        RuleFor(x => x.MaxPlayers)
            .InclusiveBetween(2, 32).WithMessage("MaxPlayers must be between 2 and 32.");
    }
}
