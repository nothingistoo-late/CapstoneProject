using FluentValidation;

namespace CapstoneProject.Application.Features.Competitive.Commands.CreateMatch;

public class CreateMatchCommandValidator : AbstractValidator<CreateMatchCommand>
{
    public CreateMatchCommandValidator()
    {
        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("Id bản đồ là bắt buộc.");
    }
}
