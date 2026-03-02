using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Gói tính năng (thời hạn, giới hạn, công cụ mở khóa). Admin tạo/sửa/bật tắt.
/// </summary>
public class Package : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int? Limit { get; set; }
    public decimal Price { get; set; }
    public string? FeaturesSpec { get; set; }

    public virtual ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
    public virtual ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
}
