using GdscSharingPlatform.Domain.Entities;
using GdscSharingPlatform.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace GdscSharingPlatform.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? StudentCode { get; set; } // ✅ Thêm thuộc tính này
    public string? Generation { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTimeOffset? JoinedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset? LastActiveAt { get; set; }
    public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    public string Locale { get; set; } = "vi-VN";
    public DateTimeOffset CreatedAt { get; set; }
        = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
    public int TokenVersion { get; set; } = 1;

}