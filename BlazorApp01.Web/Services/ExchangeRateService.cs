using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace BlazorApp01.Web.Services;

internal interface IExchangeRateService
{
    Task<IReadOnlyList<ExchangeRatePoint>> GetRatesAsync(DateTime start, DateTime end, string baseCurrency = "EUR", string symbols = "USD");
}

internal sealed record ExchangeRatePoint(DateTime DateTimeUtc, decimal Rate, DateTime CreatedAt);

internal sealed class ExchangeRateService : IExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ExchangeRateService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }
    public async Task<IReadOnlyList<ExchangeRatePoint>> GetRatesAsync(DateTime start, DateTime end, string baseCurrency = "EUR", string symbols = "USD")
    {
        using var httpClient = _httpClientFactory.CreateClient("ExchangeRate");

        // Frankfurter API uses a date range path like /{start}..{end} and query params `from` and `to`.
        var startStr = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var endStr = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var url = $"/{startStr}..{endStr}?from={baseCurrency}&to={symbols}";

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

        var createdAt = DateTime.UtcNow;
        var points = new List<ExchangeRatePoint>();

        foreach (var kvp in response.Rates.OrderBy(k => k.Key))
        {
            if (DateTime.TryParse(kvp.Key, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
            {
                var rate = kvp.Value.TryGetValue(symbols, out var r) ? (decimal)r : 0m;
                points.Add(new ExchangeRatePoint(dt, rate, createdAt));
            }
        }

        return points;
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
