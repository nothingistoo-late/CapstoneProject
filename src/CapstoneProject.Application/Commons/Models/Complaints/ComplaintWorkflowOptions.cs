namespace CapstoneProject.Application.Commons.Models.Complaints;

public class ComplaintWorkflowOptions
{
    public const string SectionName = "ComplaintWorkflow";

    public int PriorityReportWindowMinutes { get; set; } = 2 * 24 * 60;
    public int SellerResponseTimeoutMinutes { get; set; } = 24 * 60;
    public int SellerFixTimeoutMinutes { get; set; } = 3 * 24 * 60;
    public int BuyerVerificationTimeoutMinutes { get; set; } = 2 * 24 * 60;
    public int ResolvedAutoCloseMinutes { get; set; } = 60;

    public bool EnableDailyComplaintLimit { get; set; } = true;
    public int MaxReportsPerBuyerPerDay { get; set; } = 3;
    public decimal InvalidReportStrikeThresholdPercent { get; set; } = 60;

    public bool EnableAutoTransitions { get; set; } = true;
    public string AutoTransitionCron { get; set; } = "*/5 * * * *";
}
