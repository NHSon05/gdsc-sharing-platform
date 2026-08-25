namespace GdscSharingPlatform.Infrastructure.Identity;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    public RefreshToken(
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        string? createdByIp,
        string? userAgent
    )
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id is required",
                nameof(userId)
            );
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException(
                "Token hash is required",
                nameof(tokenHash)
            );
        }
        if (expiresAt <= createdAt)
        {
            throw new ArgumentException(
                "Refresh token expiration must be after its creation time.",
                nameof(expiresAt)
            );
        }
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        CreatedByIp = NormalizeOptional(createdByIp);
        UserAgent = NormalizeOptional(userAgent);
    }
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public ApplicationUser User { get; private set; } = null!;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? RevocationReason { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsActive =>
        !IsRevoked &&
        ExpiresAt > DateTimeOffset.UtcNow;
    public bool IsActiveAt(DateTimeOffset utcNow)
    {
        return !IsRevoked && ExpiresAt > utcNow;
    }
    public void Revoke(
        DateTimeOffset revokedAt,
        string reason,
        string? replacedByTokenHash = null)
    {
        // Idempotent: gọi lại không gây lỗi.
        if (IsRevoked)
        {
            return;
        }

        if (revokedAt < CreatedAt)
        {
            throw new ArgumentException(
                "Revocation time cannot be before creation time.",
                nameof(revokedAt));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "Revocation reason is required.",
                nameof(reason));
        }

        IsRevoked = true;
        RevokedAt = revokedAt;
        RevocationReason = reason.Trim();
        ReplacedByTokenHash = NormalizeOptional(replacedByTokenHash);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}