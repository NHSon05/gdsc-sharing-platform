using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(
        ApplicationDbContext dbContext,
        ILogger<DepartmentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<DepartmentDetailDto> CreateDepartmentAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name.Trim();
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

        var nameExists = await _dbContext.Departments
            .AnyAsync(d => d.Name.ToLower() == trimmedName.ToLower(), cancellationToken);
        if (nameExists)
        {
            throw new ConflictException($"Department with name '{trimmedName}' already exists.");
        }

        var slugExists = await _dbContext.Departments
            .AnyAsync(d => d.Slug.ToLower() == normalizedSlug, cancellationToken);
        if (slugExists)
        {
            throw new ConflictException($"Department with slug '{normalizedSlug}' already exists.");
        }

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = normalizedSlug.ToUpperInvariant().Replace('-', '_'),
            Name = trimmedName,
            Slug = normalizedSlug,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Color = string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim(),
            Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim(),
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created department {DepartmentId} - {DepartmentName}", department.Id, department.Name);

        return MapToDto(department);
    }

    public async Task<DepartmentDetailDto> UpdateDepartmentAsync(
        Guid departmentId,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .SingleOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        if (department is null)
        {
            throw new NotFoundException(nameof(Department), departmentId);
        }

        var trimmedName = request.Name.Trim();
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

        var nameExists = await _dbContext.Departments
            .AnyAsync(d => d.Id != departmentId && d.Name.ToLower() == trimmedName.ToLower(), cancellationToken);
        if (nameExists)
        {
            throw new ConflictException($"Department with name '{trimmedName}' already exists.");
        }

        var slugExists = await _dbContext.Departments
            .AnyAsync(d => d.Id != departmentId && d.Slug.ToLower() == normalizedSlug, cancellationToken);
        if (slugExists)
        {
            throw new ConflictException($"Department with slug '{normalizedSlug}' already exists.");
        }

        department.Name = trimmedName;
        department.Slug = normalizedSlug;
        department.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        department.Color = string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim();
        department.Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim();
        department.SortOrder = request.SortOrder;
        department.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated department {DepartmentId} - {DepartmentName}", department.Id, department.Name);

        return MapToDto(department);
    }

    public async Task DeactivateDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .SingleOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        if (department is null)
        {
            throw new NotFoundException(nameof(Department), departmentId);
        }

        department.IsActive = false;
        department.DeletedAt = DateTimeOffset.UtcNow;
        department.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated department {DepartmentId}", departmentId);
    }

    public async Task<DepartmentDetailDto> ActivateDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

        if (department is null)
        {
            throw new NotFoundException(nameof(Department), departmentId);
        }

        department.IsActive = true;
        department.DeletedAt = null;
        department.IsDeleted = false;
        department.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated department {DepartmentId}", departmentId);

        return MapToDto(department);
    }

    private static DepartmentDetailDto MapToDto(Department d) =>
        new(d.Id, d.Name, d.Slug, d.Description, d.Color, d.Icon, d.SortOrder, d.IsActive);
}
