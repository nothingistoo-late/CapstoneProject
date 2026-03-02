using FluentValidation;

namespace CapstoneProject.Application.Features.Community.Commands.DismissReport;

public class DismissReportCommandValidator : AbstractValidator<DismissReportCommand>
{
    public DismissReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("Report Id is required.");
    }
}
