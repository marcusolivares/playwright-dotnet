# Playwright + C# Blueprint

A starting point for end-to-end UI and API test suites in **C# with Playwright for .NET**. It gives you a layered structure, the plumbing every suite ends up needing, and a few working example tests that show how the pieces fit.

The examples run against [Practice Software Testing](https://practicesoftwaretesting.com) (the "Toolshop"), a public demo app built for test-automation practice. It was picked because it has a UI, a JSON API with token auth and `data-test` attributes, so a fresh clone runs green. To test your own app, replace the example layer and keep the core (see [Adapting the blueprint](#adapting-the-blueprint-to-your-app)).

| Concern | Choice |
|---|---|
| Runtime | .NET 10 (LTS) |
| Browser automation | [Microsoft.Playwright](https://playwright.dev/dotnet/) 1.64 |
| Test runner | NUnit 4 + [Microsoft.Playwright.NUnit](https://playwright.dev/dotnet/docs/test-runners), with NUnit.Analyzers |
| Test data | [Bogus](https://github.com/bchavez/Bogus) factories and JSON datasets |
| Configuration | `Microsoft.Extensions.Configuration`: JSON files plus environment variables |
| Database (optional) | ADO.NET `DbDataSource` (Npgsql by default) |
| CI | GitHub Actions |

---

## Architecture

### Two projects, one dependency direction

```
tests/Blueprint.Tests  ──references──►  src/Blueprint.Framework
  (what to test)                          (how to talk to the app)
```

- **`Blueprint.Framework`** is a class library with page objects, components, API clients, test data, database access, configuration and utilities. It has **no test-runner dependency**: it never references NUnit and never asserts. The same library could back MSTest or xUnit tests.
- **`Blueprint.Tests`** holds the NUnit tests and the infrastructure that runs them: base classes, lifecycle, auth reuse, failure artifacts and clean-up.

Because the split is enforced by the compiler, framework code can't depend on test code by accident.

### Layout

```
.
├── Blueprint.slnx
├── Directory.Build.props          # shared build settings (net10.0, nullable, analyzers, warnings-as-errors on CI)
├── Directory.Packages.props       # every NuGet version, in one place (Central Package Management)
├── global.json                    # pins the .NET SDK band
├── .editorconfig                  # code style, enforced by `dotnet format` in CI
├── .github/workflows/ci.yml
├── src/Blueprint.Framework/
│   ├── Pages/          BasePage + one page object per screen
│   ├── Components/     fragments shared by several pages (header, toasts, ...)
│   ├── Api/            ApiClient facade, ResourceClient base, ApiResponse
│   │   ├── Clients/    one client per API resource
│   │   └── Contracts/  request/response DTOs
│   ├── TestData/       TestDataFiles loader
│   │   ├── Factories/  Bogus factories for valid, unique entities
│   │   └── Files/      JSON datasets and upload fixtures (copied to the output folder)
│   ├── Models/         domain types shared by pages, clients and factories
│   ├── Database/       optional DB access (parameterized SQL only)
│   ├── Configuration/  TestSettings + layered loader
│   ├── Constants/      routes and UI copy
│   └── Utilities/      small, app-agnostic helpers
└── tests/Blueprint.Tests/
    ├── Infrastructure/ base classes, auth state, run user, artifacts, clean-up
    ├── Ui/             browser tests, grouped by feature
    ├── Api/            API tests
    ├── GlobalSetup.cs  loads configuration once per run
    ├── appsettings.json
    └── .runsettings    browser, workers, timeouts
```

### Layers

**Core** marks pieces that work for any app and that you keep. **Example** marks pieces written for the demo app, which you replace.

| Layer | Core | Example | Responsibility and rules |
|---|---|---|---|
| **Pages** | `BasePage` | `HomePage`, `LoginPage`, `AccountPage`, `ProductPage` | Own the locators and expose user-level actions (`SearchAsync`, `LoginAsync`). **Never assert**; expose locators so tests can. Navigation waits for a ready indicator. |
| **Components** | | `NavBar`, `Toasts` | Fragments that appear on many pages, so their locators live in one place. |
| **Api** | `ApiClient` pattern, `ResourceClient`, `ApiResponse`, `ApiException`, `AccessTokenProvider` | `AuthClient`, `UsersClient`, `ProductsClient`, `FavoritesClient`, `Contracts/*` | One client per resource. Return status and body; tests assert on them. Arrange steps call `EnsureSuccess()` so a broken setup reports what the server said. |
| **TestData** | `TestDataFiles` | `UserFactory`, `LoginValidationCase`, `Files/*.json` | Factories build **valid, unique** entities (customize with `with { ... }`). JSON files hold curated cases for data-driven tests. |
| **Models** | `Credentials` | `UserAccount` | Plain records. Secrets are redacted from `ToString()`. |
| **Database** | `DatabaseClient` | | Optional, for setup and verification the API can't do. Parameterized SQL only. |
| **Configuration** | `TestSettings`, `TestSettingsLoader` | | Typed settings, validated at start-up. Tests never read raw config. |
| **Constants** | | `Routes`, `UiTexts` | Paths and user-facing copy, so a wording change is a one-line fix. |
| **Utilities** | `UniqueId`, `StringExtensions`, `JsonDefaults` | | Small helpers with no app knowledge. |
| **Infrastructure** (tests) | `UiTestBase`, `AuthenticatedUiTestBase`, `ApiTestBase`, `AuthState`, `CleanupStack`, `TestArtifacts` | `TestUsers` | Test lifecycle. Tests inherit from a base class and stay focused on behaviour. |

### Design principles

These follow Playwright's own guidance ([best practices](https://playwright.dev/docs/best-practices), [POM](https://playwright.dev/dotnet/docs/pom), [auth](https://playwright.dev/dotnet/docs/auth), [API testing](https://playwright.dev/dotnet/docs/api-testing)):

1. **Test what users see.** Prefer role, label and test-id locators over CSS. `GetByTestId` targets the app's own attribute (`TestIdAttribute`: `data-test` here, `data-testid` by default).
2. **Every test is isolated.** Each test gets a fresh `BrowserContext` (no shared cookies or storage), creates its own uniquely named data (`UniqueId`, factories) and registers clean-up for it. That is why the suite can run **in parallel** (`NumberOfTestWorkers`).
3. **Web-first assertions.** `await Expect(locator).ToHaveTextAsync(...)` retries until it passes or times out. Never use `Thread.Sleep` or poll by hand.
4. **Arrange through the API, act and assert through the UI.** API setup is fast and stable. Verify side effects through the API too, for example after clicking "add to favorites".
5. **Authenticate once.** `AuthState` signs in through the UI once, saves the storage state and loads it into every authenticated test's context. It re-signs before the session expires (`StorageStateMaxAgeMinutes`).
6. **No shared accounts.** `TestUsers` registers the run's customer through the API, so there are no stored passwords and no fights over (or lockouts of) a shared account.
7. **Fail fast and loud.** Missing settings stop the run before a browser starts. Lookups throw instead of silently falling back. Clean-up failures are reported.
8. **Debuggable failures.** A failing test gets a Playwright trace, a full-page screenshot and optionally a video, all attached to the NUnit result.

---

## Getting started

Prerequisites:
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) (`pwsh`), used by Playwright's install script. On Windows, `powershell` works too.

```bash
dotnet build
pwsh tests/Blueprint.Tests/bin/Debug/net10.0/playwright.ps1 install   # downloads the browsers (once per Playwright version)
dotnet test
```

## Configuration

Settings are layered. Later sources override earlier ones:

1. `tests/Blueprint.Tests/appsettings.json`: committed defaults, **no secrets**
2. `appsettings.{E2E_ENVIRONMENT}.json`: per environment, used when `E2E_ENVIRONMENT` is set (e.g. `Staging`)
3. `appsettings.local.json`: your machine only, git-ignored (start from `appsettings.local.example.json`)
4. Environment variables prefixed `E2E_`, with `__` between sections: how CI injects secrets

| Setting | Example env var | Purpose |
|---|---|---|
| `BaseUrl` / `ApiUrl` | `E2E_BaseUrl` | App and API roots |
| `TestIdAttribute` | `E2E_TestIdAttribute` | Attribute `GetByTestId()` targets |
| `Locale`, `Viewport:Width/Height` | `E2E_Viewport__Width` | Browser context |
| `Artifacts:TraceOnFailure / ScreenshotOnFailure / VideoOnFailure` | `E2E_Artifacts__VideoOnFailure` | What to keep for failing tests |
| `Auth:ReuseStorageState`, `Auth:StorageStateMaxAgeMinutes` | `E2E_Auth__ReuseStorageState` | Sign in once (default) or before every test |
| `Database:ConnectionString` | `E2E_Database__ConnectionString` | Optional DB access |

Runner settings (browser, headless, workers, assertion timeout) live in `.runsettings` and can be overridden after `--`.

## Running tests

```bash
dotnet test                                                    # everything, headless Chromium, 4 workers
dotnet test --filter "TestCategory=Smoke"                      # by category: Smoke, UI, API
dotnet test --filter "FullyQualifiedName~LoginTests"           # one fixture
dotnet test -- Playwright.BrowserName=firefox                  # chromium | firefox | webkit
dotnet test -- Playwright.LaunchOptions.Headless=false         # watch it run
dotnet test -- NUnit.NumberOfTestWorkers=1                     # no parallelism
```

Debug with the Playwright Inspector by setting `PWDEBUG=1` (PowerShell: `$env:PWDEBUG=1; dotnet test`). Generate locators by recording: `pwsh tests/Blueprint.Tests/bin/Debug/net10.0/playwright.ps1 codegen <url>`.

**Failures:** artifacts are written to `tests/Blueprint.Tests/bin/<config>/net10.0/playwright-artifacts/` and attached to the test result. Open a trace with `pwsh tests/Blueprint.Tests/bin/Debug/net10.0/playwright.ps1 show-trace <file.zip>` or at [trace.playwright.dev](https://trace.playwright.dev).

## Writing tests

```csharp
[Category(TestCategories.Ui)]
public sealed class AccountTests : AuthenticatedUiTestBase      // or UiTestBase (anonymous) / ApiTestBase
{
    [Test]
    public async Task Customer_can_add_a_product_to_favorites()
    {
        // Arrange through the API, register clean-up before acting
        var product = await Api.Products.FirstInStockAsync("hammer");
        AddCleanup(() => Api.Favorites.RemoveProductAsync(product.Id));
        var productPage = new ProductPage(Page);
        await productPage.GotoAsync(product.Id);

        // Act through the page object
        await productPage.AddToFavoritesAsync();

        // Assert with web-first assertions, then verify the side effect through the API
        await Expect(new Toasts(Page).WithText(UiTexts.ProductAddedToFavorites)).ToBeVisibleAsync();
        var favorites = (await Api.Favorites.ListAsync()).EnsureSuccess().Data;
        Assert.That(favorites.Select(f => f.ProductId), Does.Contain(product.Id));
    }
}
```

Conventions:
- **Pick the base class by need.** Use `UiTestBase` for anonymous browser tests, `AuthenticatedUiTestBase` for tests that start signed in, and `ApiTestBase` for API-only tests (no browser is launched). Override `ApiCredentialsAsync()` to choose who `Api` acts as.
- **Name tests as sentences** (`Unknown_credentials_are_rejected`) and lay them out as Arrange / Act / Assert.
- **Keep locators in page objects or components.** A test that needs a new element gets a new property on its page object.
- **Make data unique and clean it up.** Use factories or `UniqueId`, and call `AddCleanup` before the action that creates data.
- **Use `[TestCase]` or `[TestCaseSource]` for data-driven tests.** See `LoginTests` for cases loaded from JSON.
- **Assert user-visible outcomes with `Expect(...)`** and use NUnit's `Assert.That` for values (API responses, collections).

## Adapting the blueprint to your app

1. Rename `Blueprint` to your product (solution, projects, namespaces).
2. Set `BaseUrl`, `ApiUrl` and `TestIdAttribute` in `appsettings.json`.
3. Replace the **Example** column above: pages, components, clients, contracts, factories, `Routes`, `UiTexts` and the tests. Keep the **Core** pieces.
4. Decide how authenticated tests get an account. Keep `TestUsers` if your app has a sign-up API; otherwise read a dedicated test account from settings (inject its password through `E2E_` secrets) and pass it to `AuthState`.
5. Need the database? Set `Database:ConnectionString` and use `Database.ExecuteAsync(...)` from any test. Change `DatabaseClient.Create` if you don't use PostgreSQL.

## CI

`.github/workflows/ci.yml`:
- **Build and lint:** `dotnet format --verify-no-changes`, then a Release build with warnings as errors.
- **Tests:** installs only the browser it needs, runs the suite (Chromium on push and PR; pick a browser for manual runs) and uploads TRX results and failure artifacts.

## About the example target

- The example tests register a few throwaway customer accounts per run on the public demo site (with `@example.com` addresses) and add or remove a favorite. The site exists for this kind of practice and resets its data periodically.
- To keep everything local, run the app yourself from [its repository](https://github.com/testsmith-io/practice-software-testing) with Docker. Then point `BaseUrl` to `http://localhost:4200` and `ApiUrl` to `http://localhost:8091` (see `appsettings.local.example.json`).
- A public site can change or be slow. If an example breaks, compare its selectors with the app's markup before assuming the framework is at fault.
