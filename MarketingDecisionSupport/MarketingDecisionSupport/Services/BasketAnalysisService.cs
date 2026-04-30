using MarketingDecisionSupport.Models;

namespace MarketingDecisionSupport.Services;

public class BasketAnalysisService
{
    public List<BasketRule> BuildTopRules(List<TransactionRecord> transactions, int take = 10)
    {
        if (transactions.Count == 0) return new List<BasketRule>();

        var invoiceProducts = transactions
            .Where(x => !string.IsNullOrWhiteSpace(x.Invoice_ID) && !string.IsNullOrWhiteSpace(x.Product_Name) && x.Quantity > 0)
            .GroupBy(x => x.Invoice_ID)
            .Select(g => g.Select(x => x.Product_Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList())
            .Where(x => x.Count >= 2)
            .ToList();

        var totalInvoices = invoiceProducts.Count;
        if (totalInvoices == 0) return new List<BasketRule>();

        var singleCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var pairCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var invoice in invoiceProducts)
        {
            foreach (var product in invoice)
            {
                singleCounts[product] = singleCounts.TryGetValue(product, out var count) ? count + 1 : 1;
            }

            for (var i = 0; i < invoice.Count; i++)
            {
                for (var j = 0; j < invoice.Count; j++)
                {
                    if (i == j) continue;
                    var key = $"{invoice[i]}|||{invoice[j]}";
                    pairCounts[key] = pairCounts.TryGetValue(key, out var count) ? count + 1 : 1;
                }
            }
        }

        return pairCounts
            .Select(pair =>
            {
                var parts = pair.Key.Split("|||");
                var antecedent = parts[0];
                var consequent = parts[1];
                var support = (decimal)pair.Value / totalInvoices;
                var confidence = singleCounts[antecedent] == 0 ? 0 : (decimal)pair.Value / singleCounts[antecedent];
                var consequentSupport = singleCounts[consequent] == 0 ? 0 : (decimal)singleCounts[consequent] / totalInvoices;
                var lift = consequentSupport == 0 ? 0 : confidence / consequentSupport;

                return new BasketRule
                {
                    Antecedent = antecedent,
                    Consequent = consequent,
                    Support = Math.Round(support, 4),
                    Confidence = Math.Round(confidence, 4),
                    Lift = Math.Round(lift, 4)
                };
            })
            .OrderByDescending(x => x.Lift)
            .ThenByDescending(x => x.Confidence)
            .ThenByDescending(x => x.Support)
            .Take(take)
            .ToList();
    }
}
