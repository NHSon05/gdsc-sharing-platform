using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Profile.Interfaces;
using GdscSharingPlatform.Application.Features.Profile.Models;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Services;

public sealed class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IFileStorageService fileStorageService,
        ILogger<ProfileService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<ProfileDto> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.ClubMemberships)
                .ThenInclude(cm => cm.Generation)
            .Include(u => u.ClubMemberships)
                .ThenInclude(cm => cm.DepartmentMemberships)
                    .ThenInclude(dm => dm.Department)
            .Include(u => u.ClubMemberships)
                .ThenInclude(cm => cm.DepartmentMemberships)
                    .ThenInclude(dm => dm.RoleAssignments)
                        .ThenInclude(ra => ra.ClubRole)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(nameof(ApplicationUser), userId);
        }

        return await MapToProfileDtoAsync(user);
    }

    public async Task<ProfileDto> UpdateMyProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundException(nameof(ApplicationUser), userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                var existingUserWithEmail = await _userManager.FindByEmailAsync(normalizedEmail);
                if (existingUserWithEmail != null && existingUserWithEmail.Id != userId)
                {
                    throw new ConflictException($"Email '{normalizedEmail}' is already in use by another user.");
                }

                user.Email = normalizedEmail;
                user.NormalizedEmail = normalizedEmail.ToUpperInvariant();
                user.UserName = normalizedEmail;
                user.NormalizedUserName = normalizedEmail.ToUpperInvariant();
                user.EmailConfirmed = true;
            }
        }

        var normalizedStudentCode = string.IsNullOrWhiteSpace(request.StudentCode)
            ? null
            : request.StudentCode.Trim().ToUpperInvariant();

        if (normalizedStudentCode != null)
        {
            var isDuplicate = await _dbContext.Users
                .AnyAsync(u => u.Id != userId && u.StudentCode == normalizedStudentCode, cancellationToken);

            if (isDuplicate)
            {
                throw new ConflictException($"Student code '{normalizedStudentCode}' is already in use by another member.");
            }
        }

        if (request.DisplayName is not null)
        {
            user.DisplayName = request.DisplayName.Trim();
        }

        if (request.PhoneNumber is not null)
        {
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        }

        if (request.StudentCode is not null)
        {
            user.StudentCode = normalizedStudentCode;
        }

        if (request.GithubUrl is not null)
        {
            user.GitHubUrl = string.IsNullOrWhiteSpace(request.GithubUrl) ? null : request.GithubUrl.Trim();
        }

        if (request.Bio is not null)
        {
            user.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ApplicationValidationException("profile", string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("Updated profile for user {UserId}", userId);

        return await GetMyProfileAsync(userId, cancellationToken);
    }

    public async Task<ProfileDto> ChangeEmailAsync(
        Guid userId,
        ChangeEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundException(nameof(ApplicationUser), userId);
        }

        var normalizedEmail = request.NewEmail.Trim().ToLowerInvariant();
        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existingUserWithEmail = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUserWithEmail != null && existingUserWithEmail.Id != userId)
            {
                throw new ConflictException($"Email '{normalizedEmail}' is already in use by another user.");
            }

            user.Email = normalizedEmail;
            user.NormalizedEmail = normalizedEmail.ToUpperInvariant();
            user.UserName = normalizedEmail;
            user.NormalizedUserName = normalizedEmail.ToUpperInvariant();
            user.EmailConfirmed = true;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new ApplicationValidationException("email", string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation("Changed email directly for user {UserId} to {NewEmail}", userId, normalizedEmail);
        }

        return await GetMyProfileAsync(userId, cancellationToken);
    }

    public async Task<AvatarUploadResponse> UploadAvatarAsync(
        Guid userId,
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundException(nameof(ApplicationUser), userId);
        }

        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            await _fileStorageService.DeleteAvatarAsync(user.AvatarUrl, cancellationToken);
        }

        var newAvatarUrl = await _fileStorageService.UploadAvatarAsync(userId, stream, fileName, contentType, cancellationToken);

        user.AvatarUrl = newAvatarUrl;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _userManager.UpdateAsync(user);
        _logger.LogInformation("Avatar updated for user {UserId} with URL {AvatarUrl}", userId, newAvatarUrl);

        return new AvatarUploadResponse(newAvatarUrl);
    }

    public async Task DeleteAvatarAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundException(nameof(ApplicationUser), userId);
        }

        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            await _fileStorageService.DeleteAvatarAsync(user.AvatarUrl, cancellationToken);
            user.AvatarUrl = null;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);
            _logger.LogInformation("Deleted avatar for user {UserId}", userId);
        }
    }

    private async Task<ProfileDto> MapToProfileDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var systemRole = roles.Contains(RoleNames.Admin) ? "ADMIN" : "MEMBER";

        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(user.DisplayName)) missingFields.Add("displayName");
        if (string.IsNullOrWhiteSpace(user.PhoneNumber)) missingFields.Add("phoneNumber");
        if (string.IsNullOrWhiteSpace(user.StudentCode)) missingFields.Add("studentCode");
        if (string.IsNullOrWhiteSpace(user.GitHubUrl)) missingFields.Add("githubUrl");
        if (string.IsNullOrWhiteSpace(user.Bio)) missingFields.Add("bio");
        if (string.IsNullOrWhiteSpace(user.AvatarUrl)) missingFields.Add("avatarUrl");

        var hasClubMembership = user.ClubMemberships != null && user.ClubMemberships.Count > 0;
        if (!hasClubMembership) missingFields.Add("clubMemberships");

        var allDeptMemberships = user.ClubMemberships?.SelectMany(cm => cm.DepartmentMemberships).ToList() ?? new();
        var hasDeptMembership = allDeptMemberships.Count > 0;
        if (!hasDeptMembership) missingFields.Add("departmentMemberships");

        var hasActiveRole = allDeptMemberships.SelectMany(dm => dm.RoleAssignments).Any(ra => ra.IsActive);
        if (!hasActiveRole) missingFields.Add("roleAssignments");

        var completedCount = 9 - missingFields.Count;
        var completionPercentage = (completedCount * 100) / 9;

        var membershipsDto = user.ClubMemberships?
            .OrderByDescending(cm => cm.Generation?.Number ?? 0)
            .Select(cm => new UserGenerationMembershipDto(
                cm.Id,
                new UserGenerationSummaryDto(
                    cm.Generation?.Id ?? Guid.Empty,
                    cm.Generation?.Number ?? 0,
                    cm.Generation?.Name ?? string.Empty),
                cm.IsActive,
                cm.DepartmentMemberships
                    .OrderByDescending(dm => dm.IsPrimary)
                    .ThenBy(dm => dm.Department?.SortOrder ?? 0)
                    .Select(dm => new UserDepartmentMembershipDto(
                        dm.Id,
                        new UserDepartmentSummaryDto(
                            dm.Department?.Id ?? Guid.Empty,
                            dm.Department?.Name ?? string.Empty,
                            dm.Department?.Slug ?? string.Empty),
                        dm.IsPrimary,
                        dm.RoleAssignments
                            .Where(ra => ra.IsActive)
                            .OrderBy(ra => ra.ClubRole?.Level ?? 999)
                            .Select(ra => new UserClubRoleDto(
                                ra.ClubRole?.Id ?? Guid.Empty,
                                ra.ClubRole?.Code ?? string.Empty,
                                ra.ClubRole?.Name ?? string.Empty))
                            .ToList()))
                    .ToList()))
            .ToList() ?? new List<UserGenerationMembershipDto>();

        return new ProfileDto(
            user.Id,
            user.DisplayName ?? user.FullName,
            user.Email ?? string.Empty,
            user.PhoneNumber,
            user.StudentCode,
            user.GitHubUrl,
            user.Bio,
            user.AvatarUrl,
            systemRole,
            membershipsDto,
            completionPercentage,
            missingFields,
            user.UpdatedAt);
    }
}
