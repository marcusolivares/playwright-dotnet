using Microsoft.Playwright;

namespace Blueprint.Framework.Components;

/// <summary>Transient notifications (ngx-toastr) shown after user actions.</summary>
public sealed class Toasts(IPage page)
{
    private readonly ILocator _messages = page.Locator(".toast-message");

    /// <summary>The toast with this text. Several toasts can be on screen at once, so always pick one by its text.</summary>
    public ILocator WithText(string text) => _messages.Filter(new() { HasText = text });
}
