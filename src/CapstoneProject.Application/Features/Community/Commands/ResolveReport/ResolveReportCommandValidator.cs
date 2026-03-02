using FluentValidation;

namespace CapstoneProject.Application.Features.Community.Commands.ResolveReport;

public class ResolveReportCommandValidator : AbstractValidator<ResolveReportCommand>
{
    public ResolveReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("Report Id is required.");
    }
}
