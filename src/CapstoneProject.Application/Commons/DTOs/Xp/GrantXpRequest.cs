using System.ComponentModel.DataAnnotations;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Xp;

public class GrantXpRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int Amount { get; set; }

    [Required]
    public XpSourceTypeEnum SourceType { get; set; } = XpSourceTypeEnum.AdminGrant;

    public Guid? SourceId { get; set; }

    [Required]
    [MaxLength(200)]
    public string IdempotencyKey { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Reason { get; set; } = "Admin grant";

    public string? Metadata { get; set; }
}

