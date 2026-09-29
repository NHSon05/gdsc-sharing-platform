using System.Security.Claims;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Api.Realtime;

public sealed class RealtimeAccess(ApplicationDbContext db)
{
    public static Guid UserId(ClaimsPrincipal user) => Guid.TryParse(user.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    public async Task<bool> CanReadAsync(ClaimsPrincipal user, string? group, CancellationToken ct)
    {
        var id = UserId(user);
        var admin = user.HasClaim(AuthClaimTypes.Role, RoleNames.Admin);
        if (user.Identity?.IsAuthenticated != true || id == Guid.Empty
            || (!admin && !user.HasClaim(AuthClaimTypes.Role, RoleNames.Member))
            || !int.TryParse(user.FindFirstValue(AuthClaimTypes.TokenVersion), out var version)
            || !long.TryParse(user.FindFirstValue("exp"), out var expiry)
            || expiry <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return false;
        if (!await db.Users.AsNoTracking().AnyAsync(u => u.Id == id && !u.IsDeleted
            && u.Status == UserStatus.Active && u.TokenVersion == version
            && (!u.LockoutEnabled || u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow), ct)) return false;
        if (group is null || group == $"user:{id}") return true;
        var parts = group.Split(':');
        if (parts.Length < 2 || !Guid.TryParseExact(parts[1], "D", out var entity)) return false;
        if (parts is ["content", _])
            return await db.SharingContents.AsNoTracking().AnyAsync(x => x.Id == entity
                && (admin || x.Status == SharingContentStatus.Published
                    || x.Authors.Any(a => a.UserId == id && (a.AuthorRole == SharingAuthorRole.Owner || x.Status != SharingContentStatus.Draft))), ct);
        // Same policy as the REST RSVP summary: statistics are Admin-only.
        return parts is ["schedule", _, "admins"] && admin
            && await db.SharingSchedules.AsNoTracking().AnyAsync(x => x.Id == entity, ct);
    }
}
