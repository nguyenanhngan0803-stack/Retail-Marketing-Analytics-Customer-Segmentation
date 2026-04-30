namespace MarketingDecisionSupport.Models;

public class ProcessedCustomer
{
    public int ID { get; set; }
    public decimal Income { get; set; }
    public int Age { get; set; }
    public int Recency { get; set; }
    public int NumWebVisitsMonth { get; set; }
    public int NumWebPurchases { get; set; }
    public int NumDealsPurchases { get; set; }
    public int Kidhome { get; set; }
    public int Teenhome { get; set; }
    public decimal TotalSpent { get; set; }
    public int CustomerTenureDays { get; set; }
    public decimal EmailOpenRate { get; set; }
    public decimal NpsScore { get; set; }
    public int Cluster { get; set; }
    public string ClusterName { get; set; } = string.Empty;
    public decimal ClusterConfidence { get; set; }
    public string CustomerType { get; set; } = string.Empty;
    public decimal PredictedTotalSpentNextPeriod { get; set; }
    public decimal ProbRF { get; set; }
    public decimal ProbLinearSVM { get; set; }
    public decimal ProbLDA { get; set; }
    public decimal EnsembleScore { get; set; }
    public string ModelConfidence { get; set; } = string.Empty;
    public bool DigitalEligible { get; set; }
    public decimal ExpectedGrossProfit { get; set; }
    public decimal ExpectedProfitAfterCampaign { get; set; }
    public decimal CampaignROI { get; set; }
    public string FinalAction { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
}
