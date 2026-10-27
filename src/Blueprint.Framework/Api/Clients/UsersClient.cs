using Blueprint.Framework.Api.Contracts;
using Microsoft.Playwright;

namespace Blueprint.Framework.Api.Clients;

public sealed class UsersClient(IAPIRequestContext request, AccessTokenProvider? tokens)
    : ResourceClient(request, tokens)
{
    /// <summary>Profile of the signed-in user.</summary>
    public Task<ApiResponse<UserResponse>> MeAsync() => GetAsync<UserResponse>("/users/me");
}
