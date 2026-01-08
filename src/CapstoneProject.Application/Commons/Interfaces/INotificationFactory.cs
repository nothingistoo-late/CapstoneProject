using CapstoneProject.Application.Common.Enums;

namespace CapstoneProject.Application.Common.Interfaces;

public interface INotificationFactory
{
    INotificationService GetSender(NotificationChannelEnum channel);
}