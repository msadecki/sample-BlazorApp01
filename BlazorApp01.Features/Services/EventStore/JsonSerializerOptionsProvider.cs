using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp01.Features.Services.EventStore;

/// <summary>
/// Provides centralized JSON serialization options for event sourcing.
/// </summary>
public interface IJsonSerializerOptionsProvider
{
    /// <summary>
    /// Gets the JSON serializer options for event serialization and deserialization.
    /// </summary>
    JsonSerializerOptions Options { get; }
}


internal sealed class JsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    private readonly Lazy<JsonSerializerOptions> _lazyOptions;

    public JsonSerializerOptionsProvider()
    {
        _lazyOptions = new Lazy<JsonSerializerOptions>(CreateOptions);
    }

    public JsonSerializerOptions Options => _lazyOptions.Value;

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        // Add any custom converters here if needed in the future
        // options.Converters.Add(new CustomConverter());

        return options;
    }
}