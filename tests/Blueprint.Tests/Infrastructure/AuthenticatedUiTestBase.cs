namespace Blueprint.Tests.Infrastructure;

/// <summary>
/// Browser tests that start signed in as <see cref="User"/>, with <see cref="UiTestBase.Api"/> acting as that user too.
/// By default the browser sign-in happens once and its storage state is reused (see <see cref="AuthState"/>);
/// set Auth:ReuseStorageState=false to sign in through the UI before every test instead.
/// </summary>
public abstract class AuthenticatedUiTestBase : UiTestBase
{
    protected UserAccount User { get; private set; } = null!;

    [SetUp]
    public async Task SignInThroughUiWhenNotReusingStateAsync()
    {
        if (!Settings.Auth.ReuseStorageState)
        {
            await new LoginPage(Page).LoginAsync(User.ToCredentials());
        }
    }

    protected override async Task<Credentials?> ApiCredentialsAsync()
    {
        User = await TestUsers.GetRunUserAsync(Playwright);
        return User.ToCredentials();
    }

    protected override async Task<BrowserNewContextOptions> CreateContextOptionsAsync()
    {
        var options = await base.CreateContextOptionsAsync();

        if (Settings.Auth.ReuseStorageState)
        {
            User = await TestUsers.GetRunUserAsync(Playwright);
            options.StorageStatePath = await AuthState.GetStorageStatePathAsync(
                Browser,
                key: User.Email,
                signInAsync: page => new LoginPage(page).LoginAsync(User.ToCredentials()));
        }

        return options;
    }
}
