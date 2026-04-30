namespace MarketingDecisionSupport.Models;

public class BasketRule
{
    public string Antecedent { get; set; } = string.Empty;
    public string Consequent { get; set; } = string.Empty;
    public decimal Support { get; set; }
    public decimal Confidence { get; set; }
    public decimal Lift { get; set; }
}
