using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.Commons.DTOs.Complaints;

public class SendComplaintMessageRequest
{
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}

