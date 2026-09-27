using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Sharing;

public sealed class ContentLikeConfiguration : IEntityTypeConfiguration<ContentLike>
{
    public void Configure(EntityTypeBuilder<ContentLike> b)
    {
        b.ToTable("ContentLikes");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.HasIndex(x => new { x.SharingContentId, x.UserId }).IsUnique();
        b.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        b.HasOne<SharingContent>().WithMany().HasForeignKey(x => x.SharingContentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class SavedContentConfiguration : IEntityTypeConfiguration<SavedContent>
{
    public void Configure(EntityTypeBuilder<SavedContent> b)
    {
        b.ToTable("SavedContents");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.HasIndex(x => new { x.SharingContentId, x.UserId }).IsUnique();
        b.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        b.HasOne<SharingContent>().WithMany().HasForeignKey(x => x.SharingContentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ContentCommentConfiguration : IEntityTypeConfiguration<ContentComment>
{
    public void Configure(EntityTypeBuilder<ContentComment> b)
    {
        b.ToTable("ContentComments", t =>
        {
            t.HasCheckConstraint("CK_ContentComments_Status", "\"Status\" IN (0,1,2)");
            t.HasCheckConstraint("CK_ContentComments_Version", "\"Version\" >= 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.BodyMarkdown).HasColumnType("text").HasMaxLength(2_000).IsRequired();
        b.Property(x => x.Status).HasConversion<int>().IsRequired();
        b.Property(x => x.ModerationReason).HasMaxLength(2_000);
        b.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.HasIndex(x => new { x.SharingContentId, x.Status, x.CreatedAtUtc });
        b.HasIndex(x => new { x.ParentCommentId, x.CreatedAtUtc });
        b.HasIndex(x => new { x.AuthorUserId, x.CreatedAtUtc });
        b.HasOne<SharingContent>().WithMany().HasForeignKey(x => x.SharingContentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ModeratedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ContentComment>().WithMany().HasForeignKey(x => x.ParentCommentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ScheduleRsvpConfiguration : IEntityTypeConfiguration<ScheduleRsvp>
{
    public void Configure(EntityTypeBuilder<ScheduleRsvp> b)
    {
        b.ToTable("ScheduleRsvps", t =>
        {
            t.HasCheckConstraint("CK_ScheduleRsvps_Status", "\"Status\" IN (0,1,2)");
            t.HasCheckConstraint("CK_ScheduleRsvps_Version", "\"Version\" >= 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<int>().IsRequired();
        b.Property(x => x.RespondedAtUtc).IsRequired();
        b.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        b.HasIndex(x => new { x.SharingScheduleId, x.UserId }).IsUnique();
        b.HasIndex(x => new { x.SharingScheduleId, x.Status });
        b.HasOne<SharingSchedule>().WithMany().HasForeignKey(x => x.SharingScheduleId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<int>().IsRequired();
        b.Property(x => x.EntityType).HasMaxLength(80).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Message).HasMaxLength(1_000).IsRequired();
        b.Property(x => x.Route).HasMaxLength(2_048);
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.HasIndex(x => new { x.RecipientUserId, x.EventId }).IsUnique();
        b.HasIndex(x => new { x.RecipientUserId, x.IsRead, x.CreatedAtUtc });
        b.HasIndex(x => new { x.RecipientUserId, x.CreatedAtUtc });
        b.HasIndex(x => new { x.Type, x.ActorUserId, x.EntityId, x.RecipientUserId, x.CreatedAtUtc });
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.RecipientUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("OutboxMessages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasMaxLength(120).IsRequired();
        b.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.OccurredAtUtc).IsRequired();
        b.Property(x => x.LastError).HasMaxLength(2_000);
        b.HasIndex(x => new { x.ProcessedAtUtc, x.NextAttemptAtUtc, x.OccurredAtUtc });
    }
}
