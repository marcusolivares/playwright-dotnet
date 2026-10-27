# CLAUDE.md

Guidance for Claude Code in this repository: a C# + Playwright for .NET test-automation blueprint (NUnit, .NET 10).

## Commands

```bash
dotnet build
pwsh tests/Blueprint.Tests/bin/Debug/net10.0/playwright.ps1 install   # browsers, once
dotnet test                                                          # all tests
dotnet test --filter "FullyQualifiedName~LoginTests"                 # one fixture
dotnet test --filter "TestCategory=Smoke"                            # Smoke | UI | API
dotnet test -- Playwright.BrowserName=firefox Playwright.LaunchOptions.Headless=false
dotnet format                                                        # CI runs it with --verify-no-changes
```

## Where code goes

- `src/Blueprint.Framework`: everything that talks to the app. Never reference NUnit here and never assert here.
  - `Pages/`: one page object per screen, deriving from `BasePage`. Locators are get-only properties initialized from the primary-constructor `page`; actions are `...Async` methods; navigation goes through `NavigateAsync(route)`.
  - `Components/`: fragments shared across pages.
  - `Api/Clients/`: one `ResourceClient` subclass per API resource; DTOs go in `Api/Contracts/` (snake_case on the wire via `JsonDefaults.Api`). Register new clients in `ApiClient`.
  - `TestData/Factories/`: Bogus factories returning valid, unique records. `TestData/Files/`: JSON cases and fixtures.
  - `Constants/`: `Routes` and `UiTexts`. No selectors there; selectors belong to page objects.
- `tests/Blueprint.Tests`: tests by feature under `Ui/` and `Api/`; lifecycle code in `Infrastructure/`.

## Test conventions

- Inherit from `UiTestBase` (anonymous), `AuthenticatedUiTestBase` (starts signed in as `User`) or `ApiTestBase`.
- Arrange via `Api`, act via page objects, assert with `Expect(...)` (UI) or `Assert.That` (values).
- Locator priority: role and label, then test id (`GetByTestId`, attribute `data-test`), then CSS as a last resort.
- Tests must be parallel-safe: unique data (`UniqueId`, factories) and `AddCleanup(...)` for anything created.
- Never use wrong-password attempts against a real account (the demo app locks accounts); use an unregistered `UserFactory.Create()`.
- No `Thread.Sleep` or `WaitForTimeoutAsync`; rely on auto-waiting and web-first assertions.
- Test names are sentences: `Unknown_credentials_are_rejected`.

## Configuration

`appsettings.json`, then `appsettings.{E2E_ENVIRONMENT}.json`, then `appsettings.local.json` (git-ignored), then `E2E_*` env vars. Add new settings as typed properties on `TestSettings`. Runner options (browser, workers, expect timeout) are in `.runsettings`.
