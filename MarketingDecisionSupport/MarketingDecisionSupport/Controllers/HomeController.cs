using MarketingDecisionSupport.Models;
using MarketingDecisionSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketingDecisionSupport.Controllers;

public class HomeController : Controller
{
    private readonly AnalyticsStateService _stateService;

    public HomeController(AnalyticsStateService stateService)
    {
        _stateService = stateService;
    }

    public IActionResult Index()
    {
        var snapshot = _stateService.Snapshot;
        var vm = new HomeDashboardViewModel { Snapshot = snapshot };

        if (snapshot != null)
        {
            vm.TopCustomers = snapshot.Customers.Take(10).ToList();
            vm.ActionCounts = snapshot.Customers
                .GroupBy(x => x.FinalAction)
                .ToDictionary(x => x.Key, x => x.Count());
            vm.AvgSpendByAction = snapshot.Customers
                .GroupBy(x => x.FinalAction)
                .ToDictionary(x => x.Key, x => Math.Round(x.Average(c => c.PredictedTotalSpentNextPeriod), 2));
            vm.AvgRoiByAction = snapshot.Customers
                .GroupBy(x => x.FinalAction)
                .ToDictionary(x => x.Key, x => Math.Round(x.Average(c => c.CampaignROI), 2));
        }

        return View(vm);
    }

    public IActionResult Error()
    {
        return View();
    }
}
