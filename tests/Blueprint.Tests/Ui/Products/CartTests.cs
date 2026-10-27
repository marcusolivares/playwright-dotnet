namespace Blueprint.Tests.Ui.Products;

[Category(TestCategories.Ui)]
public sealed class CartTests : UiTestBase
{
    [Test]
    public async Task Adding_a_product_updates_the_cart()
    {
        // Arrange through the API: faster and sturdier than browsing the catalog to find a product.
        var product = await Api.Products.FirstInStockAsync("pliers");
        var productPage = new ProductPage(Page);
        await productPage.GotoAsync(product.Id);

        await productPage.AddToCartAsync();

        await Expect(new Toasts(Page).WithText(UiTexts.ProductAddedToCart)).ToBeVisibleAsync();
        await Expect(new NavBar(Page).CartQuantity).ToHaveTextAsync("1");
    }
}
