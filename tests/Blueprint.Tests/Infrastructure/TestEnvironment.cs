using Blueprint.Framework.Configuration;
using Blueprint.Framework.Database;

namespace Blueprint.Tests.Infrastructure;

/// <summary>Run-wide singletons, created once by <see cref="GlobalSetup"/>.</summary>
public static class TestEnvironment
{
    private static TestSettings? _settings;
    private static DatabaseClient? _database;

    public static TestSettings Settings =>
        _settings ?? throw new InvalidOperationException("Settings are loaded by GlobalSetup; keep tests under the Blueprint.Tests namespace.");

    /// <summary>Optional database access; available when Database:ConnectionString is set.</summary>
    public static DatabaseClient Database =>
        _database ?? throw new InvalidOperationException("Set Database:ConnectionString to use the database layer.");

    internal static void Initialize(TestSettings settings)
    {
        _settings = settings;

        if (!string.IsNullOrWhiteSpace(settings.Database.ConnectionString))
        {
            _database = DatabaseClient.Create(settings.Database.ConnectionString);
        }
    }

    internal static async Task DisposeAsync()
    {
        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }
}
