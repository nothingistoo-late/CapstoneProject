using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Commands.UpdateXpPolicyConfig;

public record UpdateXpPolicyConfigCommand(
    string PolicyKey,
    bool IsEnabled,
    int Priority,
    string? ConfigJson) : IRequest<Result>;

