using System.Globalization;
using MarketingDecisionSupport.Models;
using Microsoft.VisualBasic.FileIO;

namespace MarketingDecisionSupport.Services;

public class CsvImportService
{
    public async Task<List<CustomerRaw>> ReadCustomersAsync(Stream fileStream)
    {
        using var memory = new MemoryStream();
        await fileStream.CopyToAsync(memory);
        memory.Position = 0;

        using var parser = new TextFieldParser(memory)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = true
        };
        parser.SetDelimiters(",");

        if (parser.EndOfData)
        {
            return new List<CustomerRaw>();
        }

        var header = parser.ReadFields() ?? Array.Empty<string>();
        var map = BuildMap(header);
        var rows = new List<CustomerRaw>();

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields() ?? Array.Empty<string>();
            if (fields.Length == 0) continue;

            rows.Add(new CustomerRaw
            {
                ID = GetInt(fields, map, "ID"),
                Year_Birth = GetNullableInt(fields, map, "Year_Birth"),
                Education = GetString(fields, map, "Education"),
                Marital_Status = GetString(fields, map, "Marital_Status"),
                Income = GetNullableDecimal(fields, map, "Income"),
                Kidhome = GetInt(fields, map, "Kidhome"),
                Teenhome = GetInt(fields, map, "Teenhome"),
                Dt_Customer = GetDate(fields, map, "Dt_Customer"),
                Recency = GetInt(fields, map, "Recency"),
                MntWines = GetDecimal(fields, map, "MntWines"),
                MntFruits = GetDecimal(fields, map, "MntFruits"),
                MntMeatProducts = GetDecimal(fields, map, "MntMeatProducts"),
                MntFishProducts = GetDecimal(fields, map, "MntFishProducts"),
                MntSweetProducts = GetDecimal(fields, map, "MntSweetProducts"),
                MntGoldProds = GetDecimal(fields, map, "MntGoldProds"),
                NumDealsPurchases = GetInt(fields, map, "NumDealsPurchases"),
                NumWebPurchases = GetInt(fields, map, "NumWebPurchases"),
                NumCatalogPurchases = GetInt(fields, map, "NumCatalogPurchases"),
                NumStorePurchases = GetInt(fields, map, "NumStorePurchases"),
                NumWebVisitsMonth = GetInt(fields, map, "NumWebVisitsMonth"),
                Response = GetInt(fields, map, "Response"),
                Email_Open_Rate = GetDecimal(fields, map, "Email_Open_Rate"),
                NPS_Score = GetDecimal(fields, map, "NPS_Score")
            });
        }

        return rows;
    }

    public async Task<List<TransactionRecord>> ReadTransactionsAsync(Stream fileStream)
    {
        using var memory = new MemoryStream();
        await fileStream.CopyToAsync(memory);
        memory.Position = 0;

        using var parser = new TextFieldParser(memory)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = true
        };
        parser.SetDelimiters(",");

        if (parser.EndOfData)
        {
            return new List<TransactionRecord>();
        }

        var header = parser.ReadFields() ?? Array.Empty<string>();
        var map = BuildMap(header);
        var rows = new List<TransactionRecord>();

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields() ?? Array.Empty<string>();
            if (fields.Length == 0) continue;

            rows.Add(new TransactionRecord
            {
                Invoice_ID = GetString(fields, map, "Invoice_ID") ?? string.Empty,
                Customer_ID = GetInt(fields, map, "Customer_ID"),
                Product_Name = GetString(fields, map, "Product_Name") ?? string.Empty,
                Quantity = GetDecimal(fields, map, "Quantity")
            });
        }

        return rows;
    }

    private static Dictionary<string, int> BuildMap(string[] header)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Length; i++)
        {
            var key = (header[i] ?? string.Empty).Trim();
            if (!map.ContainsKey(key)) map[key] = i;
        }
        return map;
    }

    private static string? GetString(string[] fields, Dictionary<string, int> map, string key)
    {
        return !map.TryGetValue(key, out var index) || index >= fields.Length ? null : fields[index]?.Trim();
    }

    private static int GetInt(string[] fields, Dictionary<string, int> map, string key)
    {
        var raw = GetString(fields, map, key);
        return int.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    private static int? GetNullableInt(string[] fields, Dictionary<string, int> map, string key)
    {
        var raw = GetString(fields, map, key);
        return int.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static decimal GetDecimal(string[] fields, Dictionary<string, int> map, string key)
    {
        var raw = GetString(fields, map, key);
        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0m;
    }

    private static decimal? GetNullableDecimal(string[] fields, Dictionary<string, int> map, string key)
    {
        var raw = GetString(fields, map, key);
        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static DateTime? GetDate(string[] fields, Dictionary<string, int> map, string key)
    {
        var raw = GetString(fields, map, key);
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var formats = new[] { "dd/MM/yyyy", "M/d/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };
        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(raw, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactDate))
            {
                return exactDate;
            }
        }

        return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback)
            ? fallback
            : null;
    }
}
