namespace Blueprint.Framework.Models;

/// <summary>A customer account of the app under test. Build one with <c>UserFactory.Create()</c>.</summary>
public sealed record UserAccount(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Phone,
    string Email,
    string Password)
{
    public string FullName => $"{FirstName} {LastName}";

    public Credentials ToCredentials() => new(Email, Password);

    // Keeps the password out of logs and test reports.
    public override string ToString() => $"UserAccount {{ Email = {Email} }}";
}
