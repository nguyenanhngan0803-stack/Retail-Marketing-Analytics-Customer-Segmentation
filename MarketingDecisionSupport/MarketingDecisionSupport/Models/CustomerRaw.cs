namespace MarketingDecisionSupport.Models;

public class CustomerRaw
{
    public int ID { get; set; }
    public int? Year_Birth { get; set; }
    public string? Education { get; set; }
    public string? Marital_Status { get; set; }
    public decimal? Income { get; set; }
    public int Kidhome { get; set; }
    public int Teenhome { get; set; }
    public DateTime? Dt_Customer { get; set; }
    public int Recency { get; set; }
    public decimal MntWines { get; set; }
    public decimal MntFruits { get; set; }
    public decimal MntMeatProducts { get; set; }
    public decimal MntFishProducts { get; set; }
    public decimal MntSweetProducts { get; set; }
    public decimal MntGoldProds { get; set; }
    public int NumDealsPurchases { get; set; }
    public int NumWebPurchases { get; set; }
    public int NumCatalogPurchases { get; set; }
    public int NumStorePurchases { get; set; }
    public int NumWebVisitsMonth { get; set; }
    public int Response { get; set; }
    public decimal Email_Open_Rate { get; set; }
    public decimal NPS_Score { get; set; }
}
