using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Community.Commands.ResolveReport;

public record ResolveReportCommand(Guid ReportId, string? ReviewNote = null) : IRequest<Result>;
