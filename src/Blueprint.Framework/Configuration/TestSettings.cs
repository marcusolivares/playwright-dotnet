namespace Blueprint.Framework.Configuration;

/// <summary>
/// Strongly typed test configuration, bound from appsettings*.json and E2E_* environment variables by
/// <see cref="TestSettingsLoader"/>. Add a property here for every new setting; never read raw configuration in tests.
/// </summary>
public sealed class TestSettings
{
    /// <summary>Root of the web app under test.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Root of its REST API.</summary>
    public string ApiUrl { get; init; } = string.Empty;

    /// <summary>Attribute that <c>GetByTestId()</c> targets. Playwright's default is data-testid.</summary>
    public string TestIdAttribute { get; init; } = "data-testid";

    /// <summary>Browser locale, so UI texts and number/date formats are predictable.</summary>
    public string Locale { get; init; } = "en-US";

    public ViewportSettings Viewport { get; init; } = new();

    public ArtifactSettings Artifacts { get; init; } = new();

    public AuthSettings Auth { get; init; } = new();

    public DatabaseSettings Database { get; init; } = new();

    /// <summary>Fails fast, naming every missing key, instead of letting tests fail later with vague errors.</summary>
    public void Validate()
    {
        var required = new Dictionary<string, string>
        {
            ["BaseUrl"] = BaseUrl,
            ["ApiUrl"] = ApiUrl,
            ["TestIdAttribute"] = TestIdAttribute,
        };

        var missing = required.Where(setting => string.IsNullOrWhiteSpace(setting.Value)).Select(setting => setting.Key).ToList();
        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing test settings: {string.Join(", ", missing)}. " +
                "Set them in appsettings.local.json (next to appsettings.json) or as environment variables, " +
                $"e.g. {TestSettingsLoader.EnvironmentVariablePrefix}BaseUrl.");
        }
    }
}

public sealed class ViewportSettings
{
    public int Width { get; init; } = 1280;

    public int Height { get; init; } = 720;
}

public sealed class ArtifactSettings
{
    /// <summary>Record a Playwright trace for every test; keep it only when the test fails.</summary>
    public bool TraceOnFailure { get; init; } = true;

    public bool ScreenshotOnFailure { get; init; } = true;

    /// <summary>Record a video of every test; keep it only when the test fails. Off by default (slower).</summary>
    public bool VideoOnFailure { get; init; }
}

public sealed class AuthSettings
{
    /// <summary>
    /// Sign in once and reuse the saved browser storage state in every authenticated test (fast; the approach
    /// Playwright recommends). Set to false to sign in through the UI before each authenticated test instead.
    /// </summary>
    public bool ReuseStorageState { get; init; } = true;

    /// <summary>Sign in again once the saved state is this old. Keep it below the app's session lifetime.</summary>
    public int StorageStateMaxAgeMinutes { get; init; } = 30;
}

public sealed class DatabaseSettings
{
    /// <summary>Optional. Only tests that use the database layer need it.</summary>
    public string ConnectionString { get; init; } = string.Empty;
}
