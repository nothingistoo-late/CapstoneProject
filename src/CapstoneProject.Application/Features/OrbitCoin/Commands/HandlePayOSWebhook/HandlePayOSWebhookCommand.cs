using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.HandlePayOSWebhook;

public record HandlePayOSWebhookCommand(string WebhookJson) : IRequest<bool>;
