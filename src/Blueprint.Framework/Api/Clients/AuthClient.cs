using Blueprint.Framework.Api.Contracts;
using Blueprint.Framework.Models;
using Microsoft.Playwright;

namespace Blueprint.Framework.Api.Clients;

/// <summary>Sign-up and sign-in. Both endpoints are public, so this client never sends a token.</summary>
public sealed class AuthClient(IAPIRequestContext request) : ResourceClient(request, tokens: null)
{
    public Task<ApiResponse<UserResponse>> RegisterAsync(UserAccount user) =>
        PostAsync<UserResponse>("/users/register", new RegisterUserRequest(
            user.FirstName,
            user.LastName,
            user.DateOfBirth,
            user.Phone,
            user.Email,
            user.Password));

    public Task<ApiResponse<TokenResponse>> LoginAsync(Credentials credentials) =>
        PostAsync<TokenResponse>("/users/login", credentials);

    /// <summary>Signs in and returns the bearer token; throws if the sign-in fails.</summary>
    public async Task<string> GetTokenAsync(Credentials credentials) =>
        (await LoginAsync(credentials)).EnsureSuccess().Data.AccessToken;
}
