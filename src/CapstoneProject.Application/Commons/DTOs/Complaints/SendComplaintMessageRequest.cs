using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Commons.DTOs.Complaints;

public class SendComplaintMessageRequest
{
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    public List<IFormFile>? Attachments { get; set; }
}

