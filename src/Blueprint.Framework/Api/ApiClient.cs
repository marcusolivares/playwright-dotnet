using Blueprint.Framework.Api.Clients;
using Blueprint.Framework.Models;
using Microsoft.Playwright;

namespace Blueprint.Framework.Api;

/// <summary>
/// Entry point to the API under test, grouping one client per resource. Create one per test and dispose it afterwards.
/// Use it to arrange data (much faster than through the UI), to verify side effects, and in API tests.
/// </summary>
public sealed class ApiClient : IAsyncDisposable
{
    private readonly IAPIRequestContext _request;

    private ApiClient(IAPIRequestContext request, Credentials? credentials)
    {
        _request = request;
        Auth = new AuthClient(request);

        var tokens = credentials is { } user ? new AccessTokenProvider(() => Auth.GetTokenAsync(user)) : null;
        Users = new UsersClient(request, tokens);
        Products = new ProductsClient(request);
        Favorites = new FavoritesClient(request, tokens);
    }

    public AuthClient Auth { get; }

    public UsersClient Users { get; }

    public ProductsClient Products { get; }

    public FavoritesClient Favorites { get; }

    /// <param name="playwright">The test's Playwright instance.</param>
    /// <param name="baseUrl">API root.</param>
    /// <param name="credentials">User to act as (signs in lazily, on the first call that needs a token); null for an anonymous client.</param>
    public static async Task<ApiClient> CreateAsync(IPlaywright playwright, string baseUrl, Credentials? credentials)
    {
        var request = await playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = baseUrl,
            ExtraHTTPHeaders = new Dictionary<string, string> { ["Accept"] = "application/json" },
        });

        return new ApiClient(request, credentials);
    }

    public async ValueTask DisposeAsync() => await _request.DisposeAsync();
}
