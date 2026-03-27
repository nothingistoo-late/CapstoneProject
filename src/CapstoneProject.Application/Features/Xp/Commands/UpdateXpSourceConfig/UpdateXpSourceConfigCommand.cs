using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Commands.UpdateXpSourceConfig;

public record UpdateXpSourceConfigCommand(
    XpSourceTypeEnum SourceType,
    bool IsEnabled,
    int BaseXp,
    int DailyCap,
    double BonusMultiplier,
    string? ConfigJson) : IRequest<Result>;

