using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Identity;

public class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(
        EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.CreatedAt)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.ExpiresAt)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.IsRevoked)
            .HasDefaultValue(false);

        builder.Property(refreshToken => refreshToken.ReplacedByTokenHash)
            .HasMaxLength(128);

        builder.Property(refreshToken => refreshToken.RevocationReason)
            .HasMaxLength(256);

        builder.Property(refreshToken => refreshToken.CreatedByIp)
            .HasMaxLength(64);

        builder.Property(refreshToken => refreshToken.UserAgent)
            .HasMaxLength(512);

        builder.HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        builder.HasIndex(refreshToken => new
        {
            refreshToken.UserId,
            refreshToken.IsRevoked,
            refreshToken.ExpiresAt
        });

        builder.HasOne(refreshToken => refreshToken.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
