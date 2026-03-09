namespace CapstoneProject.Application.Commons.DTOs.OrbitCoin;

public class CreateDepositOrderResult
{
    public Guid OrderId { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
}
