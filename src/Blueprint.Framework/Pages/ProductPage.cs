using Blueprint.Framework.Constants;
using Microsoft.Playwright;

namespace Blueprint.Framework.Pages;

/// <summary>Product detail page.</summary>
public sealed class ProductPage(IPage page) : BasePage(page)
{
    private readonly ILocator _addToCartButton = page.GetByTestId("add-to-cart");
    private readonly ILocator _addToFavoritesButton = page.GetByTestId("add-to-favorites");

    public ILocator Name { get; } = page.GetByTestId("product-name");

    public ILocator UnitPrice { get; } = page.GetByTestId("unit-price");

    protected override ILocator ReadyIndicator => Name;

    /// <summary>Pages with a parameterized route take the parameter in their GotoAsync.</summary>
    public Task GotoAsync(string productId) => NavigateAsync(Routes.Product(productId));

    public Task AddToCartAsync() => _addToCartButton.ClickAsync();

    public Task AddToFavoritesAsync() => _addToFavoritesButton.ClickAsync();
}
