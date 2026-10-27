using Blueprint.Framework.Configuration;
using Blueprint.Framework.Database;
using Microsoft.Playwright.NUnit;

namespace Blueprint.Tests.Infrastructure;

/// <summary>
/// Base class for API tests. No browser is launched; each test gets an <see cref="ApiClient"/> and a clean-up stack.
/// </summary>
public abstract class ApiTestBase : PlaywrightTest
{
    private readonly CleanupStack _cleanup = new();

    protected static TestSettings Settings => TestEnvironment.Settings;

    /// <summary>Optional database access; requires Database:ConnectionString.</summary>
    protected static DatabaseClient Database => TestEnvironment.Database;

    /// <summary>Anonymous unless a subclass overrides <see cref="ApiCredentialsAsync"/>.</summary>
    protected ApiClient Api { get; private set; } = null!;

    [SetUp]
    public async Task SetUpApiAsync()
    {
        Api = await ApiClient.CreateAsync(Playwright, Settings.ApiUrl, await ApiCredentialsAsync());
    }

    [TearDown]
    public async Task TearDownApiAsync()
    {
        try
        {
            await _cleanup.RunAsync();
        }
        finally
        {
            await Api.DisposeAsync();
        }
    }

    /// <summary>Registers a step to run after the test (last registered runs first), e.g. deleting what it created.</summary>
    protected void AddCleanup(Func<Task> step) => _cleanup.Push(step);

    /// <summary>Who <see cref="Api"/> acts as; null means anonymous.</summary>
    protected virtual Task<Credentials?> ApiCredentialsAsync() => Task.FromResult<Credentials?>(null);

    /// <summary>An extra client, e.g. for another user or an anonymous caller; disposed after the test.</summary>
    protected async Task<ApiClient> CreateApiAsync(Credentials? credentials)
    {
        var api = await ApiClient.CreateAsync(Playwright, Settings.ApiUrl, credentials);
        AddCleanup(() => api.DisposeAsync().AsTask());
        return api;
    }
}
