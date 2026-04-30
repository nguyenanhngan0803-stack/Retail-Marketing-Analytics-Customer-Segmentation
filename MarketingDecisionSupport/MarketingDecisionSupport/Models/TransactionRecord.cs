namespace MarketingDecisionSupport.Models;

public class TransactionRecord
{
    public string Invoice_ID { get; set; } = string.Empty;
    public int Customer_ID { get; set; }
    public string Product_Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
