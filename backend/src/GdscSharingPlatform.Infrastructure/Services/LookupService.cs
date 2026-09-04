using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services;

public sealed class LookupService : ILookupService
{
    private readonly ApplicationDbContext _dbContext;

    public LookupService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<GenerationDto>> GetGenerationsAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ClubGenerations.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(g => g.IsActive);
        }

        return await query
            .OrderByDescending(g => g.Number)
            .Select(g => new GenerationDto(
                g.Id,
                g.Number,
                g.Name,
                g.StartDate,
                g.EndDate,
                g.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DepartmentDetailDto>> GetDepartmentsAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Departments.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(d => d.IsActive && !d.IsDeleted);
        }

        return await query
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.Name)
            .Select(d => new DepartmentDetailDto(
                d.Id,
                d.Name,
                d.Slug,
                d.Description,
                d.Color,
                d.Icon,
                d.SortOrder,
                d.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClubRoleDetailDto>> GetClubRolesAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ClubRoles.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(r => r.IsActive);
        }

        return await query
            .OrderBy(r => r.Level)
            .ThenBy(r => r.Name)
            .Select(r => new ClubRoleDetailDto(
                r.Id,
                r.Code,
                r.Name,
                r.Level,
                r.IsActive))
            .ToListAsync(cancellationToken);
    }
}
