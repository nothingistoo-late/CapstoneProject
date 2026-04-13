namespace CapstoneProject.Domain.Enums;

public enum NotificationTypeEnum
{
    ComplaintCreated = 1,
    ComplaintStatusChanged = 2,
    ComplaintRefunded = 3,
    MapVersionPublished = 4,
    SystemAnnouncement = 5,
    MapRatingReceived = 6,
    AchievementUnlocked = 7,
    PaymentSucceeded = 8,
    PaymentFailed = 9,
    MapComplainedAbout = 10,        // Khi map bị complaint -> creator map nhận
    MapPurchased = 11,              // Khi có người mua map -> creator nhận
    MapUpdateForBuyers = 12,        // Khi publish map lên version mới -> người đã mua nhận
}
