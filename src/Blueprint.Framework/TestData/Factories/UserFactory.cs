using Blueprint.Framework.Models;
using Blueprint.Framework.Utilities;
using Bogus;

namespace Blueprint.Framework.TestData.Factories;

/// <summary>
/// Valid, unique users built with Bogus. Override any field with a <c>with</c> expression:
/// <c>UserFactory.Create() with { Password = "too-weak" }</c>.
/// </summary>
public static class UserFactory
{
    public static UserAccount Create()
    {
        // A new Faker per call keeps the factory thread-safe for tests running in parallel.
        var faker = new Faker();
        var firstName = faker.Name.FirstName();
        var lastName = faker.Name.LastName();
        var emailName = $"{firstName} {lastName}".ToSlug();
        var today = DateOnly.FromDateTime(DateTime.Today);

        return new UserAccount(
            FirstName: firstName,
            LastName: lastName,
            DateOfBirth: faker.Date.BetweenDateOnly(today.AddYears(-70), today.AddYears(-20)),
            Phone: faker.Phone.PhoneNumber("##########"),
            Email: $"{emailName}.{UniqueId.Next()}@example.com",
            // Random, so never on a breached-password list; the suffix guarantees mixed case, a digit and a symbol.
            Password: $"{faker.Internet.Password(12)}aA1!");
    }
}
