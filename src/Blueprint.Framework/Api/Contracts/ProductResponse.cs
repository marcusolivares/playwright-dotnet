namespace Blueprint.Framework.Api.Contracts;

/// <summary>The product fields the tests need; unknown JSON fields are ignored.</summary>
public sealed record ProductResponse(string Id, string Name, decimal Price, bool InStock);
