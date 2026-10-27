using Blueprint.Framework.Api.Contracts;
using Microsoft.Playwright;

namespace Blueprint.Framework.Api.Clients;

/// <summary>The signed-in user's favorite products.</summary>
public sealed class FavoritesClient(IAPIRequestContext request, AccessTokenProvider? tokens)
    : ResourceClient(request, tokens)
{
    public Task<ApiResponse<IReadOnlyList<FavoriteResponse>>> ListAsync() =>
        GetAsync<IReadOnlyList<FavoriteResponse>>("/favorites");

    public Task<ApiResponse<FavoriteResponse>> AddAsync(string productId) =>
        PostAsync<FavoriteResponse>("/favorites", new { ProductId = productId });

    public Task<ApiResponse> RemoveAsync(string favoriteId) => DeleteAsync($"/favorites/{favoriteId}");

    /// <summary>Idempotent clean-up: removes the product from the favorites if it is there.</summary>
    public async Task RemoveProductAsync(string productId)
    {
        var favorites = (await ListAsync()).EnsureSuccess().Data;

        foreach (var favorite in favorites.Where(favorite => favorite.ProductId == productId))
        {
            (await RemoveAsync(favorite.Id)).EnsureSuccess();
        }
    }
}
