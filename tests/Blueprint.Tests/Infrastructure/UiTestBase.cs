using Blueprint.Framework.Configuration;
using Blueprint.Framework.Database;
using Microsoft.Playwright.NUnit;

namespace Blueprint.Tests.Infrastructure;

/// <summary>
/// Base class for browser tests. Every test gets:
/// <list type="bullet">
/// <item>its own <see cref="IBrowserContext"/> and <see cref="IPage"/>, so no cookies or storage leak between tests;</item>
/// <item>an <see cref="ApiClient"/> to arrange data and verify side effects without clicking through the UI;</item>
/// <item>trace, screenshot (and optionally video) attached to the NUnit result when it fails;</item>
/// <item>a clean-up stack that runs after the test, pass or fail.</item>
/// </list>
/// </summary>
public abstract class UiTestBase : BrowserTest
{
    private readonly CleanupStack _cleanup = new();

    protected static TestSettings Settings => TestEnvironment.Settings;

    /// <summary>Optional database access; requires Database:ConnectionString.</summary>
    protected static DatabaseClient Database => TestEnvironment.Database;

    protected IBrowserContext Context { get; private set; } = null!;

    protected IPage Page { get; private set; } = null!;

    /// <summary>Anonymous unless a subclass overrides <see cref="ApiCredentialsAsync"/>.</summary>
    protected ApiClient Api { get; private set; } = null!;

    [SetUp]
    public async Task SetUpBrowserAsync()
    {
        // Lets page objects use GetByTestId() with the app's own attribute (data-test, data-cy, ...).
        Playwright.Selectors.SetTestIdAttribute(Settings.TestIdAttribute);

        Api = await ApiClient.CreateAsync(Playwright, Settings.ApiUrl, await ApiCredentialsAsync());
        Context = await NewContextAsync(await CreateContextOptionsAsync());

        if (Settings.Artifacts.TraceOnFailure)
        {
            await Context.Tracing.StartAsync(new()
            {
                Title = TestContext.CurrentContext.Test.FullName,
                Screenshots = true,
                Snapshots = true,
                Sources = true,
            });
        }

        Page = await Context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDownBrowserAsync()
    {
        var failed = TestArtifacts.CurrentTestFailed;

        try
        {
            await SaveFailureArtifactsAsync(failed);

            // Closing the context also finishes writing the video, if one is being recorded.
            await Context.DisposeAsync();
            await KeepVideoOnlyOnFailureAsync(failed);
        }
        finally
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
    }

    /// <summary>Registers a step to run after the test (last registered runs first), e.g. deleting what it created.</summary>
    protected void AddCleanup(Func<Task> step) => _cleanup.Push(step);

    /// <summary>Who <see cref="Api"/> acts as; null means anonymous.</summary>
    protected virtual Task<Credentials?> ApiCredentialsAsync() => Task.FromResult<Credentials?>(null);

    protected virtual Task<BrowserNewContextOptions> CreateContextOptionsAsync() =>
        Task.FromResult(new BrowserNewContextOptions
        {
            BaseURL = Settings.BaseUrl,
            Locale = Settings.Locale,
            ViewportSize = new() { Width = Settings.Viewport.Width, Height = Settings.Viewport.Height },
            RecordVideoDir = Settings.Artifacts.VideoOnFailure ? TestArtifacts.DirectoryFor("videos") : null,
        });

    private async Task SaveFailureArtifactsAsync(bool failed)
    {
        if (failed && Settings.Artifacts.ScreenshotOnFailure)
        {
            var screenshot = TestArtifacts.FileFor("screenshots", ".png");
            await Page.ScreenshotAsync(new() { Path = screenshot, FullPage = true });
            TestContext.AddTestAttachment(screenshot, "Screenshot at the moment of failure");
        }

        if (Settings.Artifacts.TraceOnFailure)
        {
            var trace = failed ? TestArtifacts.FileFor("traces", ".zip") : null;
            await Context.Tracing.StopAsync(new() { Path = trace });

            if (trace is not null)
            {
                TestContext.AddTestAttachment(trace, "Playwright trace (open with: playwright.ps1 show-trace <file>)");
            }
        }
    }

    private async Task KeepVideoOnlyOnFailureAsync(bool failed)
    {
        if (Page.Video is not { } video)
        {
            return;
        }

        if (failed)
        {
            TestContext.AddTestAttachment(await video.PathAsync(), "Video of the failed test");
        }
        else
        {
            await video.DeleteAsync();
        }
    }
}
