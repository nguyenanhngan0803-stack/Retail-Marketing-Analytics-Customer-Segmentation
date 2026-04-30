using MarketingDecisionSupport.Models;
using MarketingDecisionSupport.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketingDecisionSupport.Controllers;

public class ImportController : Controller
{
    private readonly CsvImportService _csvImportService;
    private readonly MarketingAnalyticsService _analyticsService;
    private readonly AnalyticsStateService _stateService;

    public ImportController(CsvImportService csvImportService, MarketingAnalyticsService analyticsService, AnalyticsStateService stateService)
    {
        _csvImportService = csvImportService;
        _analyticsService = analyticsService;
        _stateService = stateService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ImportViewModel model)
    {
        if (model.CustomerFile == null)
        {
            model.IsSuccess = false;
            model.Message = "Vui lòng chọn file khách hàng.";
            return View(model);
        }

        var customers = await _csvImportService.ReadCustomersAsync(model.CustomerFile.OpenReadStream());
        var transactions = model.TransactionFile != null
            ? await _csvImportService.ReadTransactionsAsync(model.TransactionFile.OpenReadStream())
            : new List<TransactionRecord>();

        if (customers.Count == 0)
        {
            model.IsSuccess = false;
            model.Message = "Không đọc được dữ liệu khách hàng. Vui lòng kiểm tra lại định dạng CSV.";
            return View(model);
        }

        var snapshot = _analyticsService.BuildSnapshot(customers, transactions);
        _stateService.SetSnapshot(snapshot);

        model.IsSuccess = true;
        model.Message = $"Đã nạp {snapshot.TotalCustomers} khách hàng và {snapshot.Transactions.Count} dòng giao dịch.";
        return View(model);
    }
}
