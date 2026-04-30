using MarketingDecisionSupport.Models;

namespace MarketingDecisionSupport.Services;

public class AnalyticsStateService
{
    public AnalyticsSnapshot? Snapshot { get; private set; }

    public void SetSnapshot(AnalyticsSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}
