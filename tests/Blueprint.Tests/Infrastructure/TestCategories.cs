namespace Blueprint.Tests.Infrastructure;

/// <summary>Category names for <c>[Category]</c>; filter with <c>dotnet test --filter "TestCategory=Smoke"</c>.</summary>
public static class TestCategories
{
    public const string Ui = "UI";
    public const string Api = "API";
    public const string Smoke = "Smoke";
}
