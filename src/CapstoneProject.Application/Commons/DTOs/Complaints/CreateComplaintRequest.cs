using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Commons.DTOs.Complaints;

public class CreateComplaintRequest
{
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CategoryKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public CreateComplaintContextRequest Context { get; set; } = new();

    public List<IFormFile>? Attachments { get; set; }
}

public class CreateComplaintContextRequest
{
    public Guid? PaymentRecordId { get; set; }
    public Guid? GameId { get; set; }
    public Guid? PackageId { get; set; }
    public Guid? SubmissionId { get; set; }
    public Guid? PlayHistoryId { get; set; }
    public Guid? XpTransactionId { get; set; }
    public Guid? OrbitCoinTransactionId { get; set; }
    public DateTime? OccurredAt { get; set; }
}

