namespace CapstoneProject.Application.Commons.DTOs.OrbitCoin;

public class CreditOrbitCoinRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
}
