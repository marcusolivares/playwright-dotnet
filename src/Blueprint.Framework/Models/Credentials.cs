namespace Blueprint.Framework.Models;

public sealed record Credentials(string Email, string Password)
{
    // Keeps the password out of logs and test reports.
    public override string ToString() => $"Credentials {{ Email = {Email} }}";
}
