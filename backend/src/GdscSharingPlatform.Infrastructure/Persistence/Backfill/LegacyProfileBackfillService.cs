using System.Text.RegularExpressions;
using GdscSharingPlatform.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Persistence.Backfill;

public sealed partial class LegacyProfileBackfillService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LegacyProfileBackfillService> _logger;

    public LegacyProfileBackfillService(
        ApplicationDbContext dbContext,
        ILogger<LegacyProfileBackfillService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [GeneratedRegex(@"^(?:[Gg][Ee][Nn]\s*)?0*([1-9]\d{0,2})$", RegexOptions.Compiled)]
    private static partial Regex GenerationRegex();

    public static bool TryParseGenerationNumber(string? generationText, out int generationNumber)
    {
        generationNumber = 0;
        if (string.IsNullOrWhiteSpace(generationText))
        {
            return false;
        }

        var match = GenerationRegex().Match(generationText.Trim());
        if (!match.Success)
        {
            return false;
        }

        return int.TryParse(match.Groups[1].Value, out generationNumber) && generationNumber is >= 1 and <= 999;
    }

    public async Task<BackfillResult> BackfillAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting legacy profile backfill...");

        var defaultRole = await _dbContext.ClubRoles
            .SingleOrDefaultAsync(r => r.Code == SystemClubRoles.CoreTeam, cancellationToken);

        if (defaultRole is null)
        {
            defaultRole = new ClubRole(
                code: SystemClubRoles.CoreTeam,
                name: "Core Team",
                level: 30,
                isActive: true);
            _dbContext.ClubRoles.Add(defaultRole);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var users = await _dbContext.Users
            .Where(u => !u.IsDeleted && (u.Generation != null || u.DepartmentId != null))
            .ToListAsync(cancellationToken);

        var result = new BackfillResult { TotalScanned = users.Count };

        foreach (var user in users)
        {
            if (!TryParseGenerationNumber(user.Generation, out var genNumber))
            {
                var reason = string.IsNullOrWhiteSpace(user.Generation)
                    ? "User has no Generation specified; skipping automatic membership assignment."
                    : $"Generation string '{user.Generation}' is invalid; skipping automatic membership assignment.";

                _logger.LogWarning(
                    "Legacy backfill skipped for user {UserId} ({Email}): {Reason}",
                    user.Id,
                    user.Email,
                    reason);

                result.SkippedUsers.Add(new SkippedUserRecord(user.Id, user.Email, user.Generation, reason));
                continue;
            }

            var generation = await _dbContext.ClubGenerations
                .SingleOrDefaultAsync(g => g.Number == genNumber, cancellationToken);

            if (generation is null)
            {
                generation = new ClubGeneration(genNumber);
                _dbContext.ClubGenerations.Add(generation);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var clubMembership = await _dbContext.ClubMemberships
                .SingleOrDefaultAsync(
                    cm => cm.UserId == user.Id && cm.GenerationId == generation.Id,
                    cancellationToken);

            DateOnly? joinedDate = user.JoinedAt.HasValue
                ? DateOnly.FromDateTime(user.JoinedAt.Value.UtcDateTime)
                : null;

            if (clubMembership is null)
            {
                clubMembership = new ClubMembership(
                    userId: user.Id,
                    generationId: generation.Id,
                    joinedAt: joinedDate,
                    isActive: true);
                _dbContext.ClubMemberships.Add(clubMembership);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            if (user.DepartmentId.HasValue)
            {
                var departmentExists = await _dbContext.Departments
                    .AnyAsync(d => d.Id == user.DepartmentId.Value && !d.IsDeleted, cancellationToken);

                if (departmentExists)
                {
                    var deptMembership = await _dbContext.DepartmentMemberships
                        .SingleOrDefaultAsync(
                            dm => dm.ClubMembershipId == clubMembership.Id && dm.DepartmentId == user.DepartmentId.Value,
                            cancellationToken);

                    if (deptMembership is null)
                    {
                        deptMembership = new DepartmentMembership(
                            clubMembershipId: clubMembership.Id,
                            departmentId: user.DepartmentId.Value,
                            isPrimary: true,
                            joinedAt: joinedDate,
                            isActive: true);
                        _dbContext.DepartmentMemberships.Add(deptMembership);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    var hasActiveRole = await _dbContext.RoleAssignments
                        .AnyAsync(
                            ra => ra.DepartmentMembershipId == deptMembership.Id && ra.ClubRoleId == defaultRole.Id && ra.IsActive,
                            cancellationToken);

                    if (!hasActiveRole)
                    {
                        var roleAssignment = new RoleAssignment(
                            departmentMembershipId: deptMembership.Id,
                            clubRoleId: defaultRole.Id,
                            assignedByUserId: null,
                            assignedAtUtc: DateTimeOffset.UtcNow,
                            isActive: true);
                        _dbContext.RoleAssignments.Add(roleAssignment);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "User {UserId} has DepartmentId {DepartmentId} which does not exist in Departments table.",
                        user.Id,
                        user.DepartmentId.Value);
                }
            }

            result.MigratedUsersCount++;
        }

        _logger.LogInformation(
            "Legacy profile backfill finished. Scanned: {TotalScanned}, Migrated: {MigratedCount}, Skipped: {SkippedCount}.",
            result.TotalScanned,
            result.MigratedUsersCount,
            result.SkippedUsers.Count);

        return result;
    }
}

public sealed class BackfillResult
{
    public int TotalScanned { get; set; }
    public int MigratedUsersCount { get; set; }
    public List<SkippedUserRecord> SkippedUsers { get; set; } = new();
}

public sealed record SkippedUserRecord(Guid UserId, string? Email, string? Generation, string Reason);
