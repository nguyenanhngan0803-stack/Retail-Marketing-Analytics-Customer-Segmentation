namespace MarketingDecisionSupport.Models;

public class HomeDashboardViewModel
{
    public AnalyticsSnapshot? Snapshot { get; set; }
    public List<ProcessedCustomer> TopCustomers { get; set; } = new();
    public Dictionary<string, int> ActionCounts { get; set; } = new();
    public Dictionary<string, decimal> AvgSpendByAction { get; set; } = new();
    public Dictionary<string, decimal> AvgRoiByAction { get; set; } = new();
}
