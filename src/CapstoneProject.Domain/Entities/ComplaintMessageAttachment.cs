using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

public class ComplaintMessageAttachment : BaseEntity
{
    public Guid ComplaintMessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long SizeBytes { get; set; }
    public int SortOrder { get; set; }

    public virtual ComplaintMessage ComplaintMessage { get; set; } = null!;
}
