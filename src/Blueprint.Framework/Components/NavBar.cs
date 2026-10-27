using Microsoft.Playwright;

namespace Blueprint.Framework.Components;

/// <summary>
/// Site header, present on every page. Components model page fragments that several pages share, so their locators
/// live in one place.
/// </summary>
public sealed class NavBar(IPage page)
{
    private readonly ILocator _signOutLink = page.GetByTestId("nav-sign-out");

    public ILocator SignInLink { get; } = page.GetByTestId("nav-sign-in");

    /// <summary>Dropdown labelled with the signed-in user's name.</summary>
    public ILocator UserMenu { get; } = page.GetByTestId("nav-menu");

    public ILocator CartQuantity { get; } = page.GetByTestId("cart-quantity");

    public async Task SignOutAsync()
    {
        await UserMenu.ClickAsync();
        await _signOutLink.ClickAsync();
    }
}
