namespace Blueprint.Tests.Infrastructure;

/// <summary>
/// The account authenticated tests act as. It is registered through the API once per test run, so the suite needs no
/// pre-existing account or stored password, and never shares (or locks) an account with anyone else.
/// Tests that change account state (e.g. password) should register their own user instead.
/// </summary>
internal static class TestUsers
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static UserAccount? _runUser;

    public static async Task<UserAccount> GetRunUserAsync(IPlaywright playwright)
    {
        await Gate.WaitAsync();
        try
        {
            return _runUser ??= await RegisterAsync(playwright);
        }
        finally
        {
            Gate.Release();
        }
    }

    private static async Task<UserAccount> RegisterAsync(IPlaywright playwright)
    {
        var user = UserFactory.Create();

        await using var api = await ApiClient.CreateAsync(playwright, TestEnvironment.Settings.ApiUrl, credentials: null);
        (await api.Auth.RegisterAsync(user)).EnsureSuccess();

        return user;
    }
}
