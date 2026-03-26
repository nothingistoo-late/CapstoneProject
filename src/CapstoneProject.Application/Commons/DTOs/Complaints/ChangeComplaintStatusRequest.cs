using System.ComponentModel.DataAnnotations;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Complaints;

public class ChangeComplaintStatusRequest
{
    [Required]
    public ComplaintStatusEnum ToStatus { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }
}

