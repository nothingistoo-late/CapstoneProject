using FluentValidation;

namespace CapstoneProject.Application.Features.Community.Commands.BatchDismissReports;

public class BatchDismissReportsCommandValidator : AbstractValidator<BatchDismissReportsCommand>
{
    public BatchDismissReportsCommandValidator()
    {
        RuleFor(x => x.ReportIds)
            .NotNull().WithMessage("Report Ids are required.")
            .NotEmpty().WithMessage("At least one Report Id is required.");
    }
}
