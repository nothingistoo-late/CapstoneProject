using FluentValidation;
using CapstoneProject.Application.Commons.DTOs.Gameplay;

namespace CapstoneProject.Application.Features.Lobby.Commands.SubmitLobbySolution;

public class SubmitLobbySolutionCommandValidator : AbstractValidator<SubmitLobbySolutionCommand>
{
    public SubmitLobbySolutionCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required.");
        When(x => x.Request != null && x.Request.IsWin != false, () =>
        {
            RuleFor(x => x.Request!)
                .Must(r => !string.IsNullOrWhiteSpace(r.AstSpec) || !string.IsNullOrWhiteSpace(r.BytecodeSpec))
                .WithMessage("Either AstSpec or BytecodeSpec must be provided.");
        });
    }
}
