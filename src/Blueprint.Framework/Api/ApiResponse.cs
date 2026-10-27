using System.Text.Json;
using Blueprint.Framework.Utilities;

namespace Blueprint.Framework.Api;

/// <summary>
/// Status code and body of an API call. API tests assert on it directly; arrange steps call
/// <see cref="EnsureSuccess"/> so a failed setup reports what the server answered.
/// </summary>
public class ApiResponse(int statusCode, string content, string url)
{
    public int StatusCode { get; } = statusCode;

    public string Content { get; } = content;

    public string Url { get; } = url;

    public bool IsSuccessStatusCode => StatusCode is >= 200 and <= 299;

    public ApiResponse EnsureSuccess()
    {
        ThrowIfFailed();
        return this;
    }

    protected void ThrowIfFailed()
    {
        if (!IsSuccessStatusCode)
        {
            throw new ApiException($"{Url} answered {StatusCode}: {Content}");
        }
    }
}

/// <summary>An <see cref="ApiResponse"/> whose JSON body deserializes to <typeparamref name="T"/>.</summary>
public sealed class ApiResponse<T>(int statusCode, string content, string url) : ApiResponse(statusCode, content, url)
    where T : class
{
    private T? _data;

    public T Data => _data ??= JsonSerializer.Deserialize<T>(Content, JsonDefaults.Api)
        ?? throw new ApiException($"{Url} answered {StatusCode} with an empty body.");

    public new ApiResponse<T> EnsureSuccess()
    {
        ThrowIfFailed();
        return this;
    }
}
