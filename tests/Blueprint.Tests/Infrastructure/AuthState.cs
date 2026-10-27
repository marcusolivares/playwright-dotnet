using System.Text.Json;

namespace Blueprint.Tests.Infrastructure;

/// <summary>
/// Signs in once and saves the browser storage state (cookies + localStorage) to a file that authenticated tests load
/// into their own fresh contexts, so they skip the login screen (https://playwright.dev/dotnet/docs/auth).
/// The state is refreshed once older than Auth:StorageStateMaxAgeMinutes, before the app's session would expire.
/// </summary>
internal static class AuthState
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly Dictionary<string, (string Path, DateTimeOffset SavedAt)> Saved = [];

    /// <param name="browser">Browser to sign in with.</param>
    /// <param name="key">Identifies the account (e.g. its e-mail); each account gets its own state file.</param>
    /// <param name="signInAsync">Signs in on the given page, typically through the login page.</param>
    public static async Task<string> GetStorageStatePathAsync(IBrowser browser, string key, Func<IPage, Task> signInAsync)
    {
        var settings = TestEnvironment.Settings;
        var maxAge = TimeSpan.FromMinutes(settings.Auth.StorageStateMaxAgeMinutes);

        await Gate.WaitAsync();
        try
        {
            if (Saved.TryGetValue(key, out var saved) && DateTimeOffset.UtcNow - saved.SavedAt < maxAge)
            {
                return saved.Path;
            }

            // Under the build output, which is git-ignored: the file holds a live session.
            var path = Path.Combine(AppContext.BaseDirectory, "playwright", ".auth", $"{key.ToSlug('-')}.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await using var context = await browser.NewContextAsync(new() { BaseURL = settings.BaseUrl, Locale = settings.Locale });
            await signInAsync(await context.NewPageAsync());
            EnsureReusable(await context.StorageStateAsync(new() { Path = path }));

            Saved[key] = (path, DateTimeOffset.UtcNow);
            return path;
        }
        finally
        {
            Gate.Release();
        }
    }

    private static void EnsureReusable(string storageState)
    {
        using var json = JsonDocument.Parse(storageState);
        var root = json.RootElement;

        var hasCookies = root.TryGetProperty("cookies", out var cookies) && cookies.GetArrayLength() > 0;
        var hasLocalStorage = root.TryGetProperty("origins", out var origins)
            && origins.EnumerateArray().Any(origin =>
                origin.TryGetProperty("localStorage", out var items) && items.GetArrayLength() > 0);

        if (!hasCookies && !hasLocalStorage)
        {
            throw new InvalidOperationException(
                "Signing in left no cookies or localStorage to reuse (the app may keep its session in sessionStorage). " +
                "Set Auth:ReuseStorageState=false to sign in through the UI before every authenticated test.");
        }
    }
}
