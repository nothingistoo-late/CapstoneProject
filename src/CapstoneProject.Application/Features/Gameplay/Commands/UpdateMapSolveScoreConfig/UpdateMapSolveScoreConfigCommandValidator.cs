using FluentValidation;

namespace CapstoneProject.Application.Features.Gameplay.Commands.UpdateGameSolveScoreConfig;

public class UpdateGameSolveScoreConfigCommandValidator : AbstractValidator<UpdateGameSolveScoreConfigCommand>
{
    public UpdateGameSolveScoreConfigCommandValidator()
    {
        RuleFor(x => x.BaseScore).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TimeScore).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StepsScore).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BlocksScore).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.BaseScore + x.TimeScore + x.StepsScore + x.BlocksScore == 100)
            .WithMessage("BaseScore + TimeScore + StepsScore + BlocksScore must equal 100.");
    }
}
