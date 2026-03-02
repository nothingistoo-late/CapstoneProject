namespace CapstoneProject.Domain.Enums;

/// <summary>
/// Trạng thái thử thách (map) trong quy trình UGC: nháp → gửi duyệt → duyệt/từ chối → xuất bản.
/// </summary>
public enum MapStatusEnum
{
    /// <summary>Nháp (chỉ author xem)</summary>
    Draft = 0,
    /// <summary>Đã gửi, chờ Admin/Moderator duyệt</summary>
    PendingReview = 1,
    /// <summary>Đã duyệt, chưa xuất bản</summary>
    Approved = 2,
    /// <summary>Bị từ chối</summary>
    Rejected = 3,
    /// <summary>Đã xuất bản lên catalog</summary>
    Published = 4
}
