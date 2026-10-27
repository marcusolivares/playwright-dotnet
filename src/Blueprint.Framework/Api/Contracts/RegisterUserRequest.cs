namespace Blueprint.Framework.Api.Contracts;

public sealed record RegisterUserRequest(
    string FirstName,
    string LastName,
    DateOnly Dob,
    string Phone,
    string Email,
    string Password)
{
    // Keeps the password out of logs and test reports.
    public override string ToString() => $"RegisterUserRequest {{ Email = {Email} }}";
}
