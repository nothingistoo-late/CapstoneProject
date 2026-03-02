using CapstoneProject.Application.Common.Models;
using MediatR;
using BatchReportResultDto = CapstoneProject.Application.Features.Community.Commands.BatchResolveReports.BatchReportResultDto;

namespace CapstoneProject.Application.Features.Community.Commands.BatchDismissReports;

public record BatchDismissReportsCommand(List<Guid> ReportIds, string? ReviewNote = null) : IRequest<Result<BatchReportResultDto>>;
