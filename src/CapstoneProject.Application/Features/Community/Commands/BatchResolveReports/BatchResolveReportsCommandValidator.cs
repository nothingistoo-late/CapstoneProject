using FluentValidation;

namespace CapstoneProject.Application.Features.Community.Commands.BatchResolveReports;

public class BatchResolveReportsCommandValidator : AbstractValidator<BatchResolveReportsCommand>
{
    public BatchResolveReportsCommandValidator()
    {
        RuleFor(x => x.ReportIds)
            .NotNull().WithMessage("Report Ids are required.")
            .NotEmpty().WithMessage("At least one Report Id is required.");
    }
}
