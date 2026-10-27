namespace Blueprint.Framework.Api;

/// <summary>Signs in on the first call that needs a token, then hands the same token to every client.</summary>
public sealed class AccessTokenProvider(Func<Task<string>> signInAsync)
{
    private readonly Lazy<Task<string>> _token = new(signInAsync);

    public Task<string> GetTokenAsync() => _token.Value;
}
