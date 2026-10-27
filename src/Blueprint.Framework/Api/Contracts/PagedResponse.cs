using System.Text.Json.Serialization;

namespace Blueprint.Framework.Api.Contracts;

/// <summary>One page of a paginated list: <c>{ "current_page": 1, "data": [ ... ], "total": 42 }</c>.</summary>
public sealed record PagedResponse<T>(
    int CurrentPage,
    [property: JsonPropertyName("data")] IReadOnlyList<T> Items,
    int Total);
