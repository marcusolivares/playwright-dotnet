using Blueprint.Framework.Constants;
using Microsoft.Playwright;

namespace Blueprint.Framework.Pages;

/// <summary>Customer account overview; only reachable when signed in.</summary>
public sealed class AccountPage(IPage page) : BasePage(page)
{
    public ILocator Title { get; } = page.GetByTestId("page-title");

    protected override ILocator ReadyIndicator => Title;

    public Task GotoAsync() => NavigateAsync(Routes.Account);
}
