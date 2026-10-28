namespace Blueprint.Tests.Api;

[Category(TestCategories.Api)]
public sealed class AuthApiTests : ApiTestBase
{
    [Test]
    [Category(TestCategories.Smoke)]
    public async Task Registered_user_can_sign_in_and_read_their_profile()
    {
        var user = UserFactory.Create();

        var registration = await Api.Auth.RegisterAsync(user);
        Assert.That(registration.StatusCode, Is.EqualTo(201), registration.Content);

        var userApi = await CreateApiAsync(user.ToCredentials());
        var profile = (await userApi.Users.MeAsync()).EnsureSuccess().Data;

        Assert.Multiple(() =>
        {
            Assert.That(profile.Email, Is.EqualTo(user.Email));
            Assert.That(profile.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(profile.LastName, Is.EqualTo(user.LastName));
        });
    }

    [Test]
    public async Task Registering_an_existing_email_is_rejected()
    {
        var existingUser = await TestUsers.GetRunUserAsync(Playwright);

        var response = await Api.Auth.RegisterAsync(existingUser);

        Assert.That(response.StatusCode, Is.EqualTo(409), response.Content);
    }

    [Test]
    public async Task Unknown_credentials_are_rejected()
    {
        var stranger = UserFactory.Create();

        var response = await Api.Auth.LoginAsync(stranger.ToCredentials());

        Assert.That(response.StatusCode, Is.EqualTo(401), response.Content);
    }
}
