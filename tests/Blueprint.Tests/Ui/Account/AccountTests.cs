namespace Blueprint.Tests.Ui.Account;

[Category(TestCategories.Ui)]
public sealed class AccountTests : AuthenticatedUiTestBase
{
    [Test]
    [Category(TestCategories.Smoke)]
    public async Task Signed_in_customer_sees_their_account()
    {
        var accountPage = new AccountPage(Page);

        await accountPage.GotoAsync();

        await Expect(accountPage.Title).ToHaveTextAsync(UiTexts.AccountTitle);
        await Expect(new NavBar(Page).UserMenu).ToContainTextAsync(User.FullName);
    }

    [Test]
    public async Task Customer_can_add_a_product_to_favorites()
    {
        var product = await Api.Products.FirstInStockAsync("hammer");
        AddCleanup(() => Api.Favorites.RemoveProductAsync(product.Id));
        var productPage = new ProductPage(Page);
        await productPage.GotoAsync(product.Id);

        await productPage.AddToFavoritesAsync();

        await Expect(new Toasts(Page).WithText(UiTexts.ProductAddedToFavorites)).ToBeVisibleAsync();

        // Verify the side effect through the API rather than trusting the toast alone.
        var favorites = (await Api.Favorites.ListAsync()).EnsureSuccess().Data;
        Assert.That(favorites.Select(favorite => favorite.ProductId), Does.Contain(product.Id));
    }
}
