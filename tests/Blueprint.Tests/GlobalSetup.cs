using Blueprint.Framework.Configuration;

namespace Blueprint.Tests;

/// <summary>
/// Runs once around the whole test run (it applies to every test under the Blueprint.Tests namespace).
/// Invalid configuration fails here, before any browser starts.
/// </summary>
[SetUpFixture]
public sealed class GlobalSetup
{
    [OneTimeSetUp]
    public void LoadEnvironment() => TestEnvironment.Initialize(TestSettingsLoader.Load());

    [OneTimeTearDown]
    public Task DisposeEnvironmentAsync() => TestEnvironment.DisposeAsync();
}
