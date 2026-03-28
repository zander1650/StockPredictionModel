using System.Globalization;
using StockPredictor.Models;

namespace StockPredictor.Services;

public class StockDataFetcher
{
    private readonly HttpClient _httpClient;

    // Free tier: 25 requests/day. Users can replace with their own key from
    // https://www.alphavantage.co/support/#api-key
    private const string DefaultApiKey = "VBVA802AME3XEPFA";

    public string ApiKey { get; set; } = DefaultApiKey;

    public StockDataFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<RawStockRow>> FetchDailyAsync(string ticker)
    {
        var url =
            $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY" +
            $"&symbol={Uri.EscapeDataString(ticker.Trim().ToUpperInvariant())}" +
            $"&outputsize=compact&datatype=csv&apikey={ApiKey}";

        var csv = await _httpClient.GetStringAsync(url);
        var rows = ParseCsv(csv);

        if (rows.Count < 60)
            throw new InvalidOperationException(
                $"Only {rows.Count} rows returned for \"{ticker}\". " +
                "Check the ticker symbol or try a different API key (get one free at alphavantage.co).");

        return rows;
    }

    private static List<RawStockRow> ParseCsv(string csv)
    {
        var trimmed = csv.TrimStart();

        // Alpha Vantage returns JSON (starting with '{') for errors instead of CSV
        if (trimmed.StartsWith('{'))
        {
            // Extract a readable message from the JSON error
            var message = ExtractApiErrorMessage(trimmed);
            throw new InvalidOperationException(message);
        }

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines.Length < 2)
            throw new InvalidOperationException("The API returned no data. Verify the ticker and API key.");

        // Expected header: timestamp,open,high,low,close,volume
        var header = lines[0].Split(',');
        int closeIdx = Array.FindIndex(header, h => h.Equals("close", StringComparison.OrdinalIgnoreCase));
        int volumeIdx = Array.FindIndex(header, h => h.Equals("volume", StringComparison.OrdinalIgnoreCase));

        if (closeIdx < 0 || volumeIdx < 0)
            throw new InvalidOperationException(
                $"Unexpected CSV format. Header: {lines[0]}");

        var rows = new List<RawStockRow>();

        // Data comes newest-first; we need oldest-first for indicator calculation
        for (int i = lines.Length - 1; i >= 1; i--)
        {
            var parts = lines[i].Split(',');
            if (parts.Length <= Math.Max(closeIdx, volumeIdx))
                continue;

            if (float.TryParse(parts[closeIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out var close) &&
                float.TryParse(parts[volumeIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out var volume))
            {
                rows.Add(new RawStockRow { Close = close, Volume = volume });
            }
        }

        return rows;
    }

    private static string ExtractApiErrorMessage(string json)
    {
        // Common Alpha Vantage error keys:
        //   "Error Message"  – invalid ticker or endpoint
        //   "Note"           – API rate limit exceeded
        //   "Information"    – general info (e.g. invalid key)
        string[] keys = ["Error Message", "Note", "Information"];

        foreach (var key in keys)
        {
            var marker = $"\"{key}\"";
            var idx = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;

            var colonIdx = json.IndexOf(':', idx + marker.Length);
            if (colonIdx < 0) continue;

            var valueStart = json.IndexOf('"', colonIdx + 1);
            if (valueStart < 0) continue;

            var valueEnd = json.IndexOf('"', valueStart + 1);
            if (valueEnd < 0) continue;

            return json.Substring(valueStart + 1, valueEnd - valueStart - 1);
        }

        return "The API returned an error. The \"demo\" key only supports a few tickers (e.g. IBM). " +
               "Get a free key at alphavantage.co/support/#api-key for any ticker.";
    }
}
