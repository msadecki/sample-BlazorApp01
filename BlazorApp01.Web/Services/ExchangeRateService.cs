using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace BlazorApp01.Web.Services;

internal interface IExchangeRateService
{
    Task<IReadOnlyList<ExchangeRatePoint>> GetRatesAsync(DateTime start, DateTime end, string fromCurrency = "EUR", string toCurrency = "USD");

    Task<IReadOnlyList<CurrencyInfo>> GetCurrenciesAsync();
}

internal sealed record CurrencyInfo(string Code, string Name);

internal sealed record ExchangeRatePoint(DateOnly Date, decimal Rate);

internal sealed class ExchangeRateService(IHttpClientFactory httpClientFactory) : IExchangeRateService
{
    public async Task<IReadOnlyList<ExchangeRatePoint>> GetRatesAsync(DateTime start, DateTime end, string fromCurrency = "EUR", string toCurrency = "USD")
    {
        using var httpClient = httpClientFactory.CreateClient("ExchangeRate");

        // Frankfurter API uses a date range path like /{start}..{end} and query params `from` and `to`.
        var startStr = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var endStr = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var url = $"/{startStr}..{endStr}?from={fromCurrency}&to={toCurrency}";

        var httpResponse = await httpClient.GetAsync(url);
        var content = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Frankfurter API returned status {httpResponse.StatusCode}: {content}");
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var response = JsonSerializer.Deserialize<FrankfurterTimeseriesResponse>(content, jsonOptions);
        if (response == null || response.Rates == null)
        {
            throw new InvalidOperationException($"Unable to parse Frankfurter API response: {content}");
        }

        var points = new List<ExchangeRatePoint>();

        foreach (var kvp in response.Rates.OrderBy(kvp => kvp.Key))
        {
            if (DateOnly.TryParse(kvp.Key, CultureInfo.InvariantCulture, out var date))
            {
                var rate = kvp.Value.TryGetValue(toCurrency, out var r) ? (decimal)r : 0m;
                points.Add(new ExchangeRatePoint(date, rate));
            }
        }

        return points;
    }

    public async Task<IReadOnlyList<CurrencyInfo>> GetCurrenciesAsync()
    {
        using var httpClient = httpClientFactory.CreateClient("ExchangeRate");

        var httpResponse = await httpClient.GetAsync("/currencies");
        var content = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Frankfurter API returned status {httpResponse.StatusCode}: {content}");
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var map = JsonSerializer.Deserialize<Dictionary<string, string>>(content, jsonOptions);
        if (map == null)
        {
            throw new InvalidOperationException($"Unable to parse currencies response: {content}");
        }

        var list = map.OrderBy(kvp => kvp.Key)
                      .Select(kvp => new CurrencyInfo(kvp.Key, kvp.Value))
                      .ToList();

        return list;
    }

    private sealed class FrankfurterTimeseriesResponse
    {
        [JsonPropertyName("base")]
        public string? Base { get; set; }

        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string? EndDate { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, Dictionary<string, double>>? Rates { get; set; }
    }
}
