namespace CapstoneProject.Domain.Enums;

public enum NotificationTypeEnum
{
    ComplaintCreated = 1,
    ComplaintStatusChanged = 2,
    ComplaintRefunded = 3,
    MapVersionPublished = 4,
    SystemAnnouncement = 5,
    GameRatingReceived = 6,
    AchievementUnlocked = 7,
    PaymentSucceeded = 8,
    PaymentFailed = 9,
    MapComplainedAbout = 10,        // Khi game bị complaint -> creator game nhận
    MapPurchased = 11,              // Khi có người mua game -> creator nhận
    MapUpdateForBuyers = 12,        // Khi publish game lên version mới -> người đã mua nhận
}
