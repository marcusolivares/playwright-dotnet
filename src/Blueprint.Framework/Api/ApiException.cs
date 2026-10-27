namespace Blueprint.Framework.Api;

/// <summary>An API call made to arrange a test did not succeed. NUnit reports it as a test error, not a failure.</summary>
public sealed class ApiException(string message) : Exception(message);
