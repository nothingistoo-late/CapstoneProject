using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Community;
using MediatR;

namespace CapstoneProject.Application.Features.Community.Commands.BatchDismissReports;

public record BatchDismissReportsCommand(List<Guid> ReportIds, string? ReviewNote = null) : IRequest<Result<BatchReportResultDto>>;
