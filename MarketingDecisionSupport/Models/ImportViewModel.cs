using Microsoft.AspNetCore.Http;

namespace MarketingDecisionSupport.Models;

public class ImportViewModel
{
    public IFormFile? CustomerFile { get; set; }
    public IFormFile? TransactionFile { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
}
