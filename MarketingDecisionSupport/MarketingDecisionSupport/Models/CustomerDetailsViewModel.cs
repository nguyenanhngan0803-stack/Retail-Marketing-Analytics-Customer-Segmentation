namespace MarketingDecisionSupport.Models;

public class CustomerDetailsViewModel
{
    public ProcessedCustomer Customer { get; set; } = new();
    public List<TransactionRecord> Transactions { get; set; } = new();
    public List<string> TopProducts { get; set; } = new();
}
