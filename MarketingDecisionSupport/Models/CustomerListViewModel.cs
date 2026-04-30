namespace MarketingDecisionSupport.Models;

public class CustomerListViewModel
{
    public string? Search { get; set; }
    public string? ActionFilter { get; set; }
    public List<ProcessedCustomer> Customers { get; set; } = new();
}
