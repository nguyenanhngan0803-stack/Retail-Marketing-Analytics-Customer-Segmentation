using MarketingDecisionSupport.Models;
using MarketingDecisionSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketingDecisionSupport.Controllers;

public class CustomersController : Controller
{
    private readonly AnalyticsStateService _stateService;

    public CustomersController(AnalyticsStateService stateService)
    {
        _stateService = stateService;
    }

    public IActionResult Index(string? search, string? actionFilter)
    {
        var snapshot = _stateService.Snapshot;
        if (snapshot == null)
        {
            return RedirectToAction("Index", "Import");
        }

        var query = snapshot.Customers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.ID.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(actionFilter))
        {
            query = query.Where(x => x.FinalAction == actionFilter);
        }

        var vm = new CustomerListViewModel
        {
            Search = search,
            ActionFilter = actionFilter,
            Customers = query.Take(300).ToList()
        };

        return View(vm);
    }

    public IActionResult Details(int id)
    {
        var snapshot = _stateService.Snapshot;
        if (snapshot == null)
        {
            return RedirectToAction("Index", "Import");
        }

        var customer = snapshot.Customers.FirstOrDefault(x => x.ID == id);
        if (customer == null)
        {
            return NotFound();
        }

        var transactions = snapshot.Transactions.Where(x => x.Customer_ID == id).ToList();
        var topProducts = transactions
            .GroupBy(x => x.Product_Name)
            .OrderByDescending(x => x.Sum(t => t.Quantity))
            .Take(5)
            .Select(x => x.Key)
            .ToList();

        var vm = new CustomerDetailsViewModel
        {
            Customer = customer,
            Transactions = transactions.Take(50).ToList(),
            TopProducts = topProducts
        };

        return View(vm);
    }
}
