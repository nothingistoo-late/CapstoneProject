using Microsoft.Extensions.DependencyInjection;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;

namespace CapstoneProject.Infrastructure.Factories;

/// <summary>
/// Factory interface for notification services
/// </summary>
public class NotificationFactory : INotificationFactory
{
    private readonly IServiceProvider _serviceProvider;
    public NotificationFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public INotificationService GetSender(NotificationChannelEnum channel)
    {
        return channel switch
        {
            NotificationChannelEnum.Email => _serviceProvider.GetRequiredService<IEmailService>(),
            NotificationChannelEnum.Firebase => throw new NotImplementedException($"Firebase notification channel is not implemented in base project"),
            _ => throw new NotImplementedException($"Notification channel {channel} is not supported or temporarily disabled")
        };
    }
}
