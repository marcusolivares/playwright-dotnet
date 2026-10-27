namespace Blueprint.Tests.Ui.Auth;

[Category(TestCategories.Ui)]
public sealed class LoginTests : UiTestBase
{
    private LoginPage _loginPage = null!;

    [SetUp]
    public async Task OpenLoginPageAsync()
    {
        _loginPage = new LoginPage(Page);
        await _loginPage.GotoAsync();
    }

    [Test]
    [Category(TestCategories.Smoke)]
    public async Task Registered_user_can_sign_in()
    {
        var user = await TestUsers.GetRunUserAsync(Playwright);

        await _loginPage.SubmitAsync(user.Email, user.Password);

        await Expect(new AccountPage(Page).Title).ToHaveTextAsync(UiTexts.AccountTitle);
        await Expect(new NavBar(Page).UserMenu).ToContainTextAsync(user.FullName);
    }

    [Test]
    public async Task Unknown_credentials_are_rejected()
    {
        // Never registered, so no real account collects failed attempts (the app locks accounts after a few).
        var stranger = UserFactory.Create();

        await _loginPage.SubmitAsync(stranger.Email, stranger.Password);

        await Expect(_loginPage.LoginError).ToHaveTextAsync(UiTexts.InvalidCredentials);
    }

    [TestCaseSource(nameof(InvalidInput))]
    public async Task Invalid_input_shows_a_field_error(LoginValidationCase invalid)
    {
        await _loginPage.SubmitAsync(invalid.Email, invalid.Password);

        await Expect(_loginPage.FieldError(invalid.Field)).ToHaveTextAsync(invalid.ExpectedError);
    }

    [Test]
    public async Task Signed_in_user_can_sign_out()
    {
        var user = await TestUsers.GetRunUserAsync(Playwright);
        var navBar = new NavBar(Page);
        await _loginPage.LoginAsync(user.ToCredentials());

        await navBar.SignOutAsync();

        await Expect(navBar.SignInLink).ToBeVisibleAsync();
    }

    // Data-driven: one test case per row of TestData/Files/login-validation-cases.json.
    private static IEnumerable<TestCaseData> InvalidInput() =>
        TestDataFiles.LoadJson<List<LoginValidationCase>>("login-validation-cases.json")
            .Select(invalid => new TestCaseData(invalid).SetArgDisplayNames(invalid.Description));
}
