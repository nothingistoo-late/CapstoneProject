using FluentValidation;

namespace CapstoneProject.Application.Features.Challenge.Commands.CreateMap;

public class CreateMapCommandValidator : AbstractValidator<CreateMapCommand>
{
    public CreateMapCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
        RuleFor(x => x.Request.Difficulty)
            .InclusiveBetween(1, 5).WithMessage("Difficulty must be between 1 and 5");
        RuleFor(x => x.Request.TimeLimitMs)
            .GreaterThan(0).WithMessage("TimeLimitMs must be positive");
    }
}
