using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Community;
using MediatR;

namespace CapstoneProject.Application.Features.Community.Commands.BatchResolveReports;

public record BatchResolveReportsCommand(List<Guid> ReportIds, string? ReviewNote = null) : IRequest<Result<BatchReportResultDto>>;
