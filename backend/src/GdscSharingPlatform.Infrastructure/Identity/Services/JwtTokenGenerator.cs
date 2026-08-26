using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Infrastructure.Identity.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GdscSharingPlatform.Infrastructure.Identity.Services;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private const int RefreshTokenSizeInBytes = 64;
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    public (string Token, int ExpiresInSeconds) GenerateAccessToken(
        Guid userId,
        string email,
        string fullName,
        IEnumerable<string> roles,
        Guid? departmentId,
        string status,
        int tokenVersion)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        ArgumentNullException.ThrowIfNull(roles);

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenExpirationMinutes);
        var expiresInSeconds = (int)TimeSpan.FromMinutes(_options.AccessTokenExpirationMinutes).TotalSeconds;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, fullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AuthClaimTypes.Status, status),
            new(AuthClaimTypes.TokenVersion, tokenVersion.ToString())
        };

        if (departmentId.HasValue)
        {
            claims.Add(
                new Claim(
                    AuthClaimTypes.DepartmentId,
                    departmentId.Value.ToString()));
        }

        var roleClaims = roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.Ordinal)
            .Select(role => new Claim("role", role));

        claims.AddRange(roleClaims);

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SecretKey)
        );

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256
        );
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: signingCredentials);
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return (token, expiresInSeconds);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(
                RefreshTokenSizeInBytes);

        return Base64UrlEncoder.Encode(randomBytes);
    }

    public string HashToken(string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);
        var rawBytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(rawBytes);
        return Convert.ToHexString(hashBytes);
    }
}