using System.Text.Json;

namespace Blueprint.Framework.Utilities;

/// <summary>Shared serializer settings, so every layer reads and writes JSON the same way.</summary>
public static class JsonDefaults
{
    /// <summary>
    /// Wire format of the API under test. The sample API uses snake_case (first_name, product_id); for a camelCase
    /// API drop the naming policy.
    /// </summary>
    public static JsonSerializerOptions Api { get; } = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    /// <summary>Format of the JSON files under TestData/Files: camelCase, case-insensitive.</summary>
    public static JsonSerializerOptions Files { get; } = new(JsonSerializerDefaults.Web);
}
