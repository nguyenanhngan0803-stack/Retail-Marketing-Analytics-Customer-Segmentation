namespace MarketingDecisionSupport.Models;

public class AnalyticsSnapshot
{
    public List<ProcessedCustomer> Customers { get; set; } = new();
    public List<TransactionRecord> Transactions { get; set; } = new();
    public List<BasketRule> BasketRules { get; set; } = new();
    public List<ActionSummary> ActionSummaries { get; set; } = new();
    public DateTime ImportedAt { get; set; } = DateTime.Now;

    public int TotalCustomers => Customers.Count;
    public int DigitalEligibleCount => Customers.Count(x => x.DigitalEligible);
    public int PriorityCount => Customers.Count(x => x.FinalAction == "A - Digital Priority");
    public decimal AvgPredictedSpend => Customers.Count == 0 ? 0 : Customers.Average(x => x.PredictedTotalSpentNextPeriod);
    public decimal AvgRoi => Customers.Count == 0 ? 0 : Customers.Average(x => x.CampaignROI);
    public decimal TotalExpectedProfit => Customers.Sum(x => x.ExpectedProfitAfterCampaign);
}
