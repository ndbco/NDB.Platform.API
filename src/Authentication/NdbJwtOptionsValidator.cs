using Microsoft.Extensions.Options;

namespace NDB.Platform.Api.Authentication;

/// <summary>
/// Fail-fast validator for <see cref="NdbJwtOptions"/>.
/// Registered via <c>ValidateOnStart()</c> — catches JWT misconfiguration at startup before any request is served.
/// </summary>
public sealed class NdbJwtOptionsValidator : IValidateOptions<NdbJwtOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, NdbJwtOptions options)
    {
        var failures = new List<string>();

        // SigningKey: required, minimum 32 characters (256-bit minimum security)
        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            failures.Add("NdbJwtOptions.SigningKey is required.");
        }
        else if (options.SigningKey.Length < 32)
        {
            failures.Add(
                $"NdbJwtOptions.SigningKey must be at least 32 characters (current: {options.SigningKey.Length}). " +
                "Use a random key of ≥256 bits for optimal security.");
        }

        // Issuer: required
        if (string.IsNullOrWhiteSpace(options.Issuer))
            failures.Add("NdbJwtOptions.Issuer is required.");

        // Audience: required
        if (string.IsNullOrWhiteSpace(options.Audience))
            failures.Add("NdbJwtOptions.Audience is required.");

        // RefreshEndpoint + RefreshBaseAddress: not validated at startup because the consuming
        // project may not use DefaultTokenRefresher (it is an optional feature).
        // These are validated at runtime inside DefaultTokenRefresher.ResolveRequestUri().

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
