using System.Text.Json;
using Blueprint.Framework.Utilities;
using Microsoft.Playwright;

namespace Blueprint.Framework.Api;

/// <summary>
/// Base for one client per API resource. Wraps Playwright's <see cref="IAPIRequestContext"/>, adds the bearer token
/// when the client has one and handles JSON, so each client only describes its endpoints.
/// </summary>
public abstract class ResourceClient(IAPIRequestContext request, AccessTokenProvider? tokens)
{
    protected async Task<ApiResponse<T>> GetAsync<T>(string path, IDictionary<string, object>? query = null)
        where T : class
    {
        var (status, content, url) = await SendAsync(HttpMethod.Get, path, body: null, query);
        return new ApiResponse<T>(status, content, url);
    }

    protected async Task<ApiResponse> PostAsync(string path, object body)
    {
        var (status, content, url) = await SendAsync(HttpMethod.Post, path, body, query: null);
        return new ApiResponse(status, content, url);
    }

    protected async Task<ApiResponse<T>> PostAsync<T>(string path, object body)
        where T : class
    {
        var (status, content, url) = await SendAsync(HttpMethod.Post, path, body, query: null);
        return new ApiResponse<T>(status, content, url);
    }

    protected async Task<ApiResponse> DeleteAsync(string path)
    {
        var (status, content, url) = await SendAsync(HttpMethod.Delete, path, body: null, query: null);
        return new ApiResponse(status, content, url);
    }

    private async Task<(int Status, string Content, string Url)> SendAsync(
        HttpMethod method,
        string path,
        object? body,
        IDictionary<string, object>? query)
    {
        var headers = new Dictionary<string, string>();
        if (tokens is not null)
        {
            headers["Authorization"] = $"Bearer {await tokens.GetTokenAsync()}";
        }

        if (body is not null)
        {
            headers["Content-Type"] = "application/json";
        }

        var response = await request.FetchAsync(path, new()
        {
            Method = method.Method,
            Headers = headers,
            Params = query,
            Data = body is null ? null : JsonSerializer.Serialize(body, JsonDefaults.Api),
        });

        return (response.Status, await response.TextAsync(), response.Url);
    }
}
