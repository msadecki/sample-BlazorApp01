using System.Text.Json.Serialization;
using System.Globalization;

namespace BlazorApp01.Web.Services;

internal interface IWeatherService
{
    Task<IReadOnlyList<WeatherPoint>> GetForecastsAsync(int days = 7, double latitude = 52.23224240174998, double longitude = 20.93381175935411);
}

internal sealed record WeatherPoint(
    DateTime DateTime,
    decimal TemperatureMin,
    decimal TemperatureMax,
    int TemperatureC,
    decimal PrecipitationSum,
    int PrecipitationProbabilityMax,
    decimal WindSpeedMax,
    int WeatherCode,
    DateTime? Sunrise,
    DateTime? Sunset,
    DateTime CreatedAtUtc);

internal sealed class WeatherService(IHttpClientFactory httpClientFactory) : IWeatherService
{
    public async Task<IReadOnlyList<WeatherPoint>> GetForecastsAsync(int days = 7, double latitude = 52.23224240174998, double longitude = 20.93381175935411)
    {
        // Use Open-Meteo API (no key required). We'll request daily temperatures for the next N days.
        using HttpClient httpClient = httpClientFactory.CreateClient("OpenMeteo");

        var utcNow = DateTime.UtcNow;

        var start = utcNow.Date;
        var end = start.AddDays(days - 1);
        var latitudeInvariant = latitude.ToString(CultureInfo.InvariantCulture);
        var longitudeInvariant = longitude.ToString(CultureInfo.InvariantCulture);
        // Request additional daily fields: precipitation, precipitation probability, wind speed, weather code, sunrise/sunset
        var dailyFields = string.Join(',', new[] { "temperature_2m_max", "temperature_2m_min", "precipitation_sum", "precipitation_probability_max", "windspeed_10m_max", "weathercode", "sunrise", "sunset" });
        var url = $"/v1/forecast?latitude={latitudeInvariant}&longitude={longitudeInvariant}&daily={dailyFields}&timezone=UTC&start_date={start:yyyy-MM-dd}&end_date={end:yyyy-MM-dd}";

        var response = await httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
        if (response == null || response.Daily == null)
        {
            return Array.Empty<WeatherPoint>();
        }

        var points = new List<WeatherPoint>(response.Daily.Time.Length);

        for (var i = 0; i < response.Daily.Time.Length; i++)
        {
            var dateTime = DateTime.Parse(response.Daily.Time[i], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            var tempMax = response.Daily.TemperatureMax.ElementAtOrDefault(i);
            var tempMin = response.Daily.TemperatureMin.ElementAtOrDefault(i);
            var precipitation = response.Daily.PrecipitationSum.ElementAtOrDefault(i);
            var precipProb = response.Daily.PrecipitationProbabilityMax.ElementAtOrDefault(i);
            var windMax = response.Daily.WindSpeedMax.ElementAtOrDefault(i);
            var weatherCode = response.Daily.WeatherCode.ElementAtOrDefault(i);

            DateTime? sunrise = null;
            DateTime? sunset = null;
            if (response.Daily.Sunrise.Length > i)
            {
                if (DateTime.TryParse(response.Daily.Sunrise[i], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var s))
                {
                    sunrise = s;
                }
            }

            if (response.Daily.Sunset.Length > i)
            {
                if (DateTime.TryParse(response.Daily.Sunset[i], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var s2))
                {
                    sunset = s2;
                }
            }

            var tempC = (int)Math.Round((tempMax + tempMin) / 2.0);
            points.Add(new WeatherPoint(
                DateTime: dateTime,
                TemperatureMin: (decimal)tempMin,
                TemperatureMax: (decimal)tempMax,
                TemperatureC: tempC,
                PrecipitationSum: (decimal)precipitation,
                PrecipitationProbabilityMax: (int)Math.Round(precipProb),
                WindSpeedMax: (decimal)windMax,
                WeatherCode: (int)Math.Round(weatherCode),
                Sunrise: sunrise,
                Sunset: sunset,
                CreatedAtUtc: utcNow));
        }

        return points;
    }

    private sealed class OpenMeteoResponse
    {
        [JsonPropertyName("daily")]
        public Daily? Daily { get; set; }
    }

    private sealed class Daily
    {
        [JsonPropertyName("time")]
        public string[] Time { get; set; } = Array.Empty<string>();

        [JsonPropertyName("temperature_2m_max")]
        public double[] TemperatureMax { get; set; } = Array.Empty<double>();

        [JsonPropertyName("temperature_2m_min")]
        public double[] TemperatureMin { get; set; } = Array.Empty<double>();

        [JsonPropertyName("precipitation_sum")]
        public double[] PrecipitationSum { get; set; } = Array.Empty<double>();

        [JsonPropertyName("precipitation_probability_max")]
        public double[] PrecipitationProbabilityMax { get; set; } = Array.Empty<double>();

        [JsonPropertyName("windspeed_10m_max")]
        public double[] WindSpeedMax { get; set; } = Array.Empty<double>();

        [JsonPropertyName("weathercode")]
        public double[] WeatherCode { get; set; } = Array.Empty<double>();

        [JsonPropertyName("sunrise")]
        public string[] Sunrise { get; set; } = Array.Empty<string>();

        [JsonPropertyName("sunset")]
        public string[] Sunset { get; set; } = Array.Empty<string>();
    }
}
