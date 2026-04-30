using MarketingDecisionSupport.Models;

namespace MarketingDecisionSupport.Services;

public class MarketingAnalyticsService
{
    private readonly BasketAnalysisService _basketAnalysisService;
    private static readonly DateTime ReferenceDate = new(2014, 12, 31);

    public MarketingAnalyticsService(BasketAnalysisService basketAnalysisService)
    {
        _basketAnalysisService = basketAnalysisService;
    }

    public AnalyticsSnapshot BuildSnapshot(List<CustomerRaw> customers, List<TransactionRecord> transactions)
    {
        if (customers.Count == 0)
        {
            return new AnalyticsSnapshot();
        }

        var validIncome = customers.Where(x => x.Income.HasValue).Select(x => x.Income!.Value).OrderBy(x => x).ToList();
        var medianIncome = validIncome.Count == 0 ? 0m : validIncome[validIncome.Count / 2];

        var processed = customers
            .Select(x => BuildProcessedCustomer(x, medianIncome))
            .Where(x => x.Age <= 90 && x.Income < 600000)
            .ToList();

        AssignClusters(processed);
        ScoreClassification(processed);
        ScoreRegression(processed);
        ApplyDigitalEligibility(processed);
        ApplyCampaignActions(processed);

        var snapshot = new AnalyticsSnapshot
        {
            Customers = processed.OrderByDescending(x => x.CampaignROI).ThenByDescending(x => x.PredictedTotalSpentNextPeriod).ToList(),
            Transactions = transactions,
            BasketRules = _basketAnalysisService.BuildTopRules(transactions),
            ImportedAt = DateTime.Now
        };

        snapshot.ActionSummaries = snapshot.Customers
            .GroupBy(x => x.FinalAction)
            .Select(g => new ActionSummary
            {
                FinalAction = g.Key,
                SoKhach = g.Count(),
                ChiTieuDuBaoTB = Math.Round(g.Average(x => x.PredictedTotalSpentNextPeriod), 2),
                RoiTrungBinh = Math.Round(g.Average(x => x.CampaignROI), 2),
                LoiNhuanKyVongTB = Math.Round(g.Average(x => x.ExpectedProfitAfterCampaign), 2)
            })
            .OrderBy(x => x.FinalAction)
            .ToList();

        return snapshot;
    }

    private static ProcessedCustomer BuildProcessedCustomer(CustomerRaw raw, decimal medianIncome)
    {
        var income = raw.Income ?? medianIncome;
        var age = raw.Year_Birth.HasValue ? 2014 - raw.Year_Birth.Value : 0;
        var totalSpent = raw.MntWines + raw.MntFruits + raw.MntMeatProducts + raw.MntFishProducts + raw.MntSweetProducts + raw.MntGoldProds;
        var tenure = raw.Dt_Customer.HasValue ? (ReferenceDate - raw.Dt_Customer.Value).Days : 0;

        return new ProcessedCustomer
        {
            ID = raw.ID,
            Income = income,
            Age = age,
            Recency = raw.Recency,
            NumWebVisitsMonth = raw.NumWebVisitsMonth,
            NumWebPurchases = raw.NumWebPurchases,
            NumDealsPurchases = raw.NumDealsPurchases,
            Kidhome = raw.Kidhome,
            Teenhome = raw.Teenhome,
            TotalSpent = totalSpent,
            CustomerTenureDays = tenure,
            EmailOpenRate = raw.Email_Open_Rate,
            NpsScore = raw.NPS_Score
        };
    }

    private static void AssignClusters(List<ProcessedCustomer> customers)
    {
        if (customers.Count < 3)
        {
            foreach (var customer in customers)
            {
                customer.Cluster = 0;
                customer.ClusterName = "Khách hàng Tiết kiệm";
                customer.ClusterConfidence = 0.5m;
                customer.CustomerType = "Core";
            }
            return;
        }

        var features = customers
            .Select(x => new[] { (double)x.Age, (double)x.TotalSpent, (double)x.Income })
            .ToList();

        var normalized = Normalize(features);
        var assignments = RunKMeans(normalized, 3, 42, out var centers);

        var clusterSpent = new Dictionary<int, decimal>();
        for (var i = 0; i < 3; i++)
        {
            var members = customers.Where((_, idx) => assignments[idx] == i).ToList();
            clusterSpent[i] = members.Count == 0 ? 0 : members.Average(x => x.TotalSpent);
        }

        var ranked = clusterSpent.OrderBy(x => x.Value).Select((x, idx) => new { x.Key, Rank = idx }).ToDictionary(x => x.Key, x => x.Rank);
        var nameMap = new Dictionary<int, string>
        {
            [0] = "Khách hàng Tiết kiệm",
            [1] = "Khách hàng Tiềm năng",
            [2] = "Khách hàng Giá trị cao"
        };

        for (var i = 0; i < customers.Count; i++)
        {
            var rank = ranked[assignments[i]];
            customers[i].Cluster = rank;
            customers[i].ClusterName = nameMap[rank];

            var distance = Euclidean(normalized[i], centers[assignments[i]]);
            var confidence = 1m - (decimal)Math.Min(distance / 2.5, 0.55);
            customers[i].ClusterConfidence = Math.Round(Math.Max(0.45m, confidence), 4);
            customers[i].CustomerType = customers[i].ClusterConfidence >= 0.6m ? "Core" : "Boundary";
        }
    }

    private static void ScoreClassification(List<ProcessedCustomer> customers)
    {
        if (customers.Count == 0) return;

        var incomeMin = customers.Min(x => x.Income);
        var incomeMax = customers.Max(x => x.Income);
        var spentMin = customers.Min(x => x.TotalSpent);
        var spentMax = customers.Max(x => x.TotalSpent);
        var webVisitMin = customers.Min(x => x.NumWebVisitsMonth);
        var webVisitMax = customers.Max(x => x.NumWebVisitsMonth);
        var webPurchaseMin = customers.Min(x => x.NumWebPurchases);
        var webPurchaseMax = customers.Max(x => x.NumWebPurchases);
        var dealsMin = customers.Min(x => x.NumDealsPurchases);
        var dealsMax = customers.Max(x => x.NumDealsPurchases);
        var ageMin = customers.Min(x => x.Age);
        var ageMax = customers.Max(x => x.Age);

        foreach (var customer in customers)
        {
            var incomeNorm = Normalize(customer.Income, incomeMin, incomeMax);
            var spentNorm = Normalize(customer.TotalSpent, spentMin, spentMax);
            var webVisitNorm = Normalize(customer.NumWebVisitsMonth, webVisitMin, webVisitMax);
            var webPurchaseNorm = Normalize(customer.NumWebPurchases, webPurchaseMin, webPurchaseMax);
            var dealNorm = Normalize(customer.NumDealsPurchases, dealsMin, dealsMax);
            var ageNorm = Normalize(customer.Age, ageMin, ageMax);
            var recencyInv = 1m - Normalize(customer.Recency, 0, 100);
            var clusterVip = customer.Cluster == 2 ? 1m : customer.Cluster == 1 ? 0.45m : 0m;

            var baseScore =
                0.90m * spentNorm +
                0.86m * recencyInv +
                0.68m * webVisitNorm +
                0.55m * webPurchaseNorm +
                0.52m * incomeNorm +
                0.35m * ageNorm +
                0.28m * dealNorm +
                0.15m * clusterVip -
                0.10m * customer.Kidhome -
                0.08m * customer.Teenhome -
                0.40m;

            var baseProbability = Sigmoid(baseScore);
            customer.ProbRF = Clamp(baseProbability + 0.03m * clusterVip + 0.02m * spentNorm);
            customer.ProbLinearSVM = Clamp(baseProbability + 0.02m * webVisitNorm + 0.01m * recencyInv);
            customer.ProbLDA = Clamp(baseProbability - 0.01m + 0.02m * incomeNorm);
            customer.EnsembleScore = Math.Round(0.50m * customer.ProbLinearSVM + 0.30m * customer.ProbRF + 0.20m * customer.ProbLDA, 4);

            var votes = 0;
            if (customer.ProbRF >= 0.36m) votes++;
            if (customer.ProbLinearSVM >= 0.24m) votes++;
            if (customer.ProbLDA >= 0.26m) votes++;

            customer.ModelConfidence = votes switch
            {
                3 => "High Confidence",
                2 => "Medium Confidence",
                1 => "Low Confidence",
                _ => "Very Low Confidence"
            };
        }
    }

    private static void ScoreRegression(List<ProcessedCustomer> customers)
    {
        if (customers.Count == 0) return;

        var incomeMin = customers.Min(x => x.Income);
        var incomeMax = customers.Max(x => x.Income);
        var spentMin = customers.Min(x => x.TotalSpent);
        var spentMax = customers.Max(x => x.TotalSpent);
        var webPurchaseMin = customers.Min(x => x.NumWebPurchases);
        var webPurchaseMax = customers.Max(x => x.NumWebPurchases);

        foreach (var customer in customers)
        {
            var incomeNorm = Normalize(customer.Income, incomeMin, incomeMax);
            var spentNorm = Normalize(customer.TotalSpent, spentMin, spentMax);
            var webPurchaseNorm = Normalize(customer.NumWebPurchases, webPurchaseMin, webPurchaseMax);
            var recencyInv = 1m - Normalize(customer.Recency, 0, 100);
            var clusterBoost = customer.Cluster == 2 ? 240m : customer.Cluster == 1 ? 90m : 0m;

            var predicted =
                320m +
                customer.TotalSpent * 0.48m +
                incomeNorm * 430m +
                clusterBoost +
                customer.NumWebVisitsMonth * 11m +
                customer.NumWebPurchases * 16m +
                recencyInv * 170m -
                customer.Kidhome * 65m -
                customer.Teenhome * 25m +
                customer.EnsembleScore * 120m +
                webPurchaseNorm * 40m;

            customer.PredictedTotalSpentNextPeriod = Math.Round(Math.Max(120m, predicted), 3);
        }
    }

    private static void ApplyDigitalEligibility(List<ProcessedCustomer> customers)
    {
        if (customers.Count == 0) return;

        var medianWeb = Median(customers.Select(x => (decimal)x.NumWebVisitsMonth));
        var medianEmail = Median(customers.Select(x => x.EmailOpenRate));

        foreach (var customer in customers)
        {
            var condRecent = customer.Recency <= 60;
            var condWeb = customer.NumWebVisitsMonth >= medianWeb;
            var condEmail = customer.EmailOpenRate >= medianEmail && medianEmail > 0;
            customer.DigitalEligible = condRecent || condWeb || condEmail;
        }
    }

    private static void ApplyCampaignActions(List<ProcessedCustomer> customers)
    {
        var eligible = customers.Where(x => x.DigitalEligible).OrderBy(x => x.PredictedTotalSpentNextPeriod).ToList();
        decimal p90 = 0, p70 = 0, p40 = 0;

        if (eligible.Count > 0)
        {
            p90 = Quantile(eligible.Select(x => x.PredictedTotalSpentNextPeriod).ToList(), 0.90m);
            p70 = Quantile(eligible.Select(x => x.PredictedTotalSpentNextPeriod).ToList(), 0.70m);
            p40 = Quantile(eligible.Select(x => x.PredictedTotalSpentNextPeriod).ToList(), 0.40m);
        }

        foreach (var customer in customers)
        {
            if (!customer.DigitalEligible)
            {
                customer.FinalAction = "D - Không ưu tiên";
                customer.RecommendedAction = "Không ưu tiên ngân sách";
                customer.ExpectedGrossProfit = Math.Round(customer.PredictedTotalSpentNextPeriod * 0.35m, 4);
                customer.ExpectedProfitAfterCampaign = Math.Round(customer.ExpectedGrossProfit, 4);
                customer.CampaignROI = 0;
                continue;
            }

            if (customer.PredictedTotalSpentNextPeriod >= p90)
            {
                customer.FinalAction = "A - Digital Priority";
                customer.RecommendedAction = "Email cá nhân hóa + remarketing";
            }
            else if (customer.PredictedTotalSpentNextPeriod >= p70)
            {
                customer.FinalAction = "B - Digital Standard";
                customer.RecommendedAction = "Email nuôi dưỡng + ưu đãi nhẹ";
            }
            else if (customer.PredictedTotalSpentNextPeriod >= p40)
            {
                customer.FinalAction = "C - Digital Test";
                customer.RecommendedAction = "Test nội dung và tệp nhỏ";
            }
            else
            {
                customer.FinalAction = "D - Không ưu tiên";
                customer.RecommendedAction = "Theo dõi thêm trước khi chạy";
            }

            var campaignCost = customer.FinalAction == "D - Không ưu tiên" ? 0m : 20m;
            customer.ExpectedGrossProfit = Math.Round(customer.PredictedTotalSpentNextPeriod * 0.35m, 4);
            customer.ExpectedProfitAfterCampaign = Math.Round(customer.ExpectedGrossProfit - campaignCost, 4);
            customer.CampaignROI = campaignCost == 0m ? 0m : Math.Round(customer.ExpectedProfitAfterCampaign / campaignCost, 6);
        }
    }

    private static List<double[]> Normalize(List<double[]> data)
    {
        var dimensions = data[0].Length;
        var mins = new double[dimensions];
        var maxs = new double[dimensions];

        for (var d = 0; d < dimensions; d++)
        {
            mins[d] = data.Min(x => x[d]);
            maxs[d] = data.Max(x => x[d]);
        }

        return data.Select(row =>
        {
            var values = new double[dimensions];
            for (var d = 0; d < dimensions; d++)
            {
                var range = maxs[d] - mins[d];
                values[d] = range == 0 ? 0 : (row[d] - mins[d]) / range;
            }
            return values;
        }).ToList();
    }

    private static int[] RunKMeans(List<double[]> points, int k, int seed, out List<double[]> centers)
    {
        var random = new Random(seed);
        centers = points.OrderBy(_ => random.Next()).Take(k).Select(x => x.ToArray()).ToList();
        var assignments = new int[points.Count];

        for (var iteration = 0; iteration < 25; iteration++)
        {
            var changed = false;
            for (var i = 0; i < points.Count; i++)
            {
                var bestCluster = 0;
                var bestDistance = double.MaxValue;
                for (var c = 0; c < k; c++)
                {
                    var distance = Euclidean(points[i], centers[c]);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestCluster = c;
                    }
                }

                if (assignments[i] != bestCluster)
                {
                    assignments[i] = bestCluster;
                    changed = true;
                }
            }

            if (!changed && iteration > 0) break;

            for (var c = 0; c < k; c++)
            {
                var members = points.Where((_, idx) => assignments[idx] == c).ToList();
                if (members.Count == 0) continue;
                var updated = new double[points[0].Length];
                for (var d = 0; d < updated.Length; d++)
                {
                    updated[d] = members.Average(x => x[d]);
                }
                centers[c] = updated;
            }
        }

        return assignments;
    }

    private static double Euclidean(double[] a, double[] b)
    {
        var sum = 0d;
        for (var i = 0; i < a.Length; i++)
        {
            sum += Math.Pow(a[i] - b[i], 2);
        }
        return Math.Sqrt(sum);
    }

    private static decimal Normalize(decimal value, decimal min, decimal max)
    {
        if (max <= min) return 0m;
        return (value - min) / (max - min);
    }

    private static decimal Sigmoid(decimal value)
    {
        var exponent = Math.Exp((double)(-value));
        return (decimal)(1d / (1d + exponent));
    }

    private static decimal Clamp(decimal value)
    {
        if (value < 0m) return 0m;
        if (value > 1m) return 1m;
        return Math.Round(value, 4);
    }

    private static decimal Median(IEnumerable<decimal> source)
    {
        var values = source.OrderBy(x => x).ToList();
        if (values.Count == 0) return 0m;
        var middle = values.Count / 2;
        return values.Count % 2 == 0 ? (values[middle - 1] + values[middle]) / 2m : values[middle];
    }

    private static decimal Quantile(List<decimal> values, decimal quantile)
    {
        if (values.Count == 0) return 0m;
        var sorted = values.OrderBy(x => x).ToList();
        var index = (sorted.Count - 1) * quantile;
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);
        if (lower == upper) return sorted[lower];
        var weight = index - lower;
        return sorted[lower] * (1 - weight) + sorted[upper] * weight;
    }
}
