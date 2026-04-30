using MarketingDecisionSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketingDecisionSupport.Controllers;

public class CampaignController : Controller
{
    private readonly AnalyticsStateService _stateService;

    public CampaignController(AnalyticsStateService stateService)
    {
        _stateService = stateService;
    }

    public IActionResult Index()
    {
        var snapshot = _stateService.Snapshot;
        if (snapshot == null)
        {
            return RedirectToAction("Index", "Import");
        }

        return View(snapshot);
    }
}
