using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Auth.Interfaces;
using GdscSharingPlatform.Application.Features.Auth.Models;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Identity.Services;

public sealed class ExternalLoginService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IUserSessionService sessionService,
    ILogger<ExternalLoginService> logger) : IExternalLoginService
{
    public async Task<AuthResponse> LoginAsync(
        VerifiedExternalIdentity identity,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        if (identity.Provider != "Google" ||
            string.IsNullOrWhiteSpace(identity.Subject))
        {
            throw new ExternalLoginException(
                "external_identity_invalid");
        }

        // PostgreSQL: tạo user, gán role, liên kết và lưu phiên
        // cùng thành công hoặc cùng rollback.
        // InMemory tests do not model transactions or relational constraints.
        await using var transaction = dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;

        var user = await userManager.FindByLoginAsync(
            identity.Provider,
            identity.Subject);

        var isNewUser = false;

        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(identity.Email) ||
                !identity.EmailVerified)
            {
                throw new ExternalLoginException(
                    "verified_email_required");
            }

            var email = identity.Email.Trim();

            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser is not null)
            {
                // Có email trùng không chứng minh user đã đồng ý
                // liên kết Google với tài khoản GDSC này.
                throw new ExternalLoginException(
                    "account_link_required");
            }

            var fullName = identity.DisplayName?.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = "Thành viên";
            }

            // Khớp giới hạn FullName trong EF configuration hiện tại.
            if (fullName.Length > 150)
            {
                fullName = fullName[..150];
            }

            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                Status = UserStatus.Active
            };

            EnsureSucceeded(
                await userManager.CreateAsync(user),
                "user_creation_failed");

            EnsureSucceeded(
                await userManager.AddToRoleAsync(user, RoleNames.Member),
                "member_role_assignment_failed");

            EnsureSucceeded(
                await userManager.AddLoginAsync(
                    user,
                    new UserLoginInfo(
                        identity.Provider,
                        identity.Subject,
                        identity.Provider)),
                "external_link_failed");

            isNewUser = true;
        }

        if (user.IsDeleted ||
            user.Status != UserStatus.Active ||
            await userManager.IsLockedOutAsync(user))
        {
            throw new ExternalLoginException(
                "account_unavailable");
        }

        var response = await sessionService.CreateAsync(
            user,
            ipAddress,
            userAgent,
            cancellationToken);

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        logger.LogInformation(
            "External login succeeded. Provider {Provider}, " +
            "UserId {UserId}, NewUser {NewUser}.",
            identity.Provider,
            user.Id,
            isNewUser);

        return response;
    }

    private static void EnsureSucceeded(
        IdentityResult result,
        string errorCode)
    {
        if (!result.Succeeded)
        {
            throw new ExternalLoginException(errorCode);
        }
    }
}
