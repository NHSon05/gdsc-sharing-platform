using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.Infrastructure.Identity.Options;

public sealed class JwtOptionValidator : IValidateOptions<JwtOptions>
{
    private const int MinimumSecretKeyLength = 32;
    public ValidateOptionsResult Validate(
        string? name,
        JwtOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            errors.Add("Jwt:Issuer is required.");
        }
        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            errors.Add("Jwt:Audience is required.");
        }
        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            errors.Add("Jwt:SecretKey is required.");
        }
        else if (options.SecretKey.Length < MinimumSecretKeyLength)
        {
            errors.Add(
                $"Jwt:SecretKey must contain at least " +
                $"{MinimumSecretKeyLength} characters.");
        }
        if (options.AccessTokenExpirationMinutes is < 1 or > 1440)
        {
            errors.Add(
                "Jwt:AccessTokenExpirationMinutes must be " +
                "between 1 and 1440.");
        }

        if (options.RefreshTokenExpirationDays is < 1 or > 365)
        {
            errors.Add(
                "Jwt:RefreshTokenExpirationDays must be " +
                "between 1 and 365.");
        }
        if (options.ClockSkewSeconds is < 0 or > 300)
        {
            errors.Add(
                "Jwt:ClockSkewSeconds must be between 0 and 300.");
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}