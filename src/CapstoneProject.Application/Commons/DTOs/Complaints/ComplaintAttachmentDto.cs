namespace CapstoneProject.Application.Commons.DTOs.Complaints;

public class ComplaintAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long SizeBytes { get; set; }
    public int SortOrder { get; set; }
}

public class ComplaintMessagePostedDto
{
    public Guid MessageId { get; set; }
    public Guid ComplaintId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<ComplaintAttachmentDto> Attachments { get; set; } = new();
}
