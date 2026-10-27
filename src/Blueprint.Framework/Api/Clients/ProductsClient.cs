using Blueprint.Framework.Api.Contracts;
using Microsoft.Playwright;

namespace Blueprint.Framework.Api.Clients;

/// <summary>Public product catalog.</summary>
public sealed class ProductsClient(IAPIRequestContext request) : ResourceClient(request, tokens: null)
{
    public Task<ApiResponse<PagedResponse<ProductResponse>>> SearchAsync(string query) =>
        GetAsync<PagedResponse<ProductResponse>>("/products/search", new Dictionary<string, object> { ["q"] = query });

    /// <summary>First product matching <paramref name="query"/> that can be bought. Throws if there is none.</summary>
    public async Task<ProductResponse> FirstInStockAsync(string query)
    {
        var products = (await SearchAsync(query)).EnsureSuccess().Data.Items;

        return products.FirstOrDefault(product => product.Stock is null or > 0)
            ?? throw new ApiException($"No product in stock matches '{query}'.");
    }
}
