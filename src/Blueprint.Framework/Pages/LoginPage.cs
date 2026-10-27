using Blueprint.Framework.Constants;
using Blueprint.Framework.Models;
using Microsoft.Playwright;

namespace Blueprint.Framework.Pages;

public sealed class LoginPage(IPage page) : BasePage(page)
{
    private readonly ILocator _submitButton = page.GetByTestId("login-submit");

    public ILocator Form { get; } = page.GetByTestId("login-form");

    public ILocator EmailInput { get; } = page.GetByTestId("email");

    public ILocator PasswordInput { get; } = page.GetByTestId("password");

    /// <summary>Form-level error, e.g. for wrong credentials.</summary>
    public ILocator LoginError { get; } = page.GetByTestId("login-error");

    protected override ILocator ReadyIndicator => Form;

    public Task GotoAsync() => NavigateAsync(Routes.Login);

    /// <summary>Validation message under a field: "email" or "password".</summary>
    public ILocator FieldError(string field) => Page.GetByTestId($"{field}-error");

    public async Task SubmitAsync(string email, string password)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
        await _submitButton.ClickAsync();
    }

    /// <summary>The whole sign-in flow; returns once the account page is displayed.</summary>
    public async Task LoginAsync(Credentials credentials)
    {
        await GotoAsync();
        await SubmitAsync(credentials.Email, credentials.Password);
        await new AccountPage(Page).WaitUntilReadyAsync();
    }
}
