namespace Blueprint.Framework.Constants;

/// <summary>Paths of the app under test, relative to the configured BaseUrl.</summary>
public static class Routes
{
    public const string Home = "/";
    public const string Login = "/auth/login";
    public const string Account = "/account";

    public static string Product(string id) => $"/product/{id}";
}
