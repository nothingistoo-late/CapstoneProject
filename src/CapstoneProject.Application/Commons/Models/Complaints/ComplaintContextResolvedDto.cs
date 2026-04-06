namespace CapstoneProject.Application.Commons.Models.Complaints;

public class ComplaintContextResolvedDto
{
    public string? DisplayTitle { get; set; }
    public string? DisplaySubtitle { get; set; }
    public string? ReferenceCode { get; set; }
    public DateTime? EventTime { get; set; }
    public decimal? AmountValue { get; set; }
    public int? DeltaValue { get; set; }
    public ComplaintLinkedOrderDto? LinkedOrder { get; set; }
}

public class ComplaintLinkedOrderDto
{
    public Guid OrderId { get; set; }
    public string? OrderCode { get; set; }
    public string? OrderStatus { get; set; }
    public decimal? AmountOrbitCoin { get; set; }
    public long? AmountVnd { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentTargetType { get; set; }
    public Guid? PaymentTargetId { get; set; }
    public string? PaymentTargetName { get; set; }
}