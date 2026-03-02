using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Community.Commands.DismissReport;

public record DismissReportCommand(Guid ReportId, string? ReviewNote = null) : IRequest<Result>;
