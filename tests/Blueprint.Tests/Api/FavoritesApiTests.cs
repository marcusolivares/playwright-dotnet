namespace Blueprint.Tests.Api;

[Category(TestCategories.Api)]
public sealed class FavoritesApiTests : ApiTestBase
{
    [Test]
    public async Task Customer_can_add_and_remove_a_favorite()
    {
        var product = await Api.Products.FirstInStockAsync("pliers");
        AddCleanup(() => Api.Favorites.RemoveProductAsync(product.Id));

        var added = await Api.Favorites.AddAsync(product.Id);
        Assert.That(added.StatusCode, Is.EqualTo(201), added.Content);
        Assert.That(await FavoriteIdsAsync(), Does.Contain(added.Data.Id));

        var removed = await Api.Favorites.RemoveAsync(added.Data.Id);
        Assert.That(removed.StatusCode, Is.EqualTo(204), removed.Content);
        Assert.That(await FavoriteIdsAsync(), Does.Not.Contain(added.Data.Id));
    }

    [Test]
    public async Task Favorites_require_authentication()
    {
        var anonymous = await CreateApiAsync(credentials: null);

        var response = await anonymous.Favorites.ListAsync();

        Assert.That(response.StatusCode, Is.EqualTo(401), response.Content);
    }

    // This fixture acts as the run's registered user instead of an anonymous caller.
    protected override async Task<Credentials?> ApiCredentialsAsync() =>
        (await TestUsers.GetRunUserAsync(Playwright)).ToCredentials();

    private async Task<IEnumerable<string>> FavoriteIdsAsync() =>
        (await Api.Favorites.ListAsync()).EnsureSuccess().Data.Select(favorite => favorite.Id);
}
