namespace Blueprint.Framework.TestData;

/// <summary>One row of Files/login-validation-cases.json.</summary>
/// <param name="Description">Readable case name, shown as the test case name.</param>
/// <param name="Field">Field expected to show the error: "email" or "password".</param>
public sealed record LoginValidationCase(
    string Description,
    string Email,
    string Password,
    string Field,
    string ExpectedError);
