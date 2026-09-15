using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Sharing;

public sealed class SharingContentConfiguration : IEntityTypeConfiguration<SharingContent>
{
    public void Configure(EntityTypeBuilder<SharingContent> b)
    {
        b.ToTable("SharingContents", t => { t.HasCheckConstraint("CK_SharingContents_Status", "\"Status\" IN (0,1,2,3,4)"); t.HasCheckConstraint("CK_SharingContents_Version", "\"Version\" >= 0"); });
        b.HasKey(x => x.Id); b.Property(x => x.Title).HasMaxLength(200).IsRequired(); b.Property(x => x.Slug).HasMaxLength(200).IsRequired(); b.Property(x => x.Summary).HasMaxLength(1000).IsRequired(); b.Property(x => x.BodyMarkdown).HasColumnType("text").IsRequired(); b.Property(x => x.CoverImageUrl).HasMaxLength(2048); b.Property(x => x.Status).HasConversion<int>().IsRequired(); b.Property(x => x.ReviewNote).HasMaxLength(4000); b.Property(x => x.Version).IsConcurrencyToken().IsRequired(); b.Property(x => x.CreatedAtUtc).IsRequired(); b.HasIndex(x => x.Slug).IsUnique(); b.HasIndex(x => new { x.Status, x.PublishedAtUtc }); b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict); b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class SharingContentAuthorConfiguration : IEntityTypeConfiguration<SharingContentAuthor>
{
    public void Configure(EntityTypeBuilder<SharingContentAuthor> b)
    {
        b.ToTable("SharingContentAuthors"); b.HasKey(x => x.Id); b.Property(x => x.AuthorRole).HasConversion<int>().IsRequired(); b.Property(x => x.SortOrder).IsRequired(); b.Property(x => x.CreatedAtUtc).IsRequired(); b.HasIndex(x => new { x.SharingContentId, x.UserId }).IsUnique(); b.HasIndex(x => new { x.UserId, x.SharingContentId }); b.HasIndex(x => x.SharingContentId).IsUnique().HasFilter("\"AuthorRole\" = 0"); b.HasOne<SharingContent>().WithMany(x => x.Authors).HasForeignKey(x => x.SharingContentId).OnDelete(DeleteBehavior.Restrict); b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class SharingResourceConfiguration : IEntityTypeConfiguration<SharingResource>
{
    public void Configure(EntityTypeBuilder<SharingResource> b)
    {
        b.ToTable("SharingResources", t => t.HasCheckConstraint("CK_SharingResources_SortOrder", "\"SortOrder\" >= 0")); b.HasKey(x => x.Id); b.Property(x => x.Title).HasMaxLength(200).IsRequired(); b.Property(x => x.Description).HasMaxLength(1000); b.Property(x => x.ResourceType).HasConversion<int>().IsRequired(); b.Property(x => x.OriginalFileName).HasMaxLength(255); b.Property(x => x.StorageKey).HasMaxLength(500); b.Property(x => x.ContentType).HasMaxLength(255); b.Property(x => x.ExternalUrl).HasMaxLength(2048); b.Property(x => x.IsActive).HasDefaultValue(true).IsRequired(); b.HasIndex(x => new { x.SharingContentId, x.IsActive, x.SortOrder }); b.HasOne<SharingContent>().WithMany(x => x.Resources).HasForeignKey(x => x.SharingContentId).OnDelete(DeleteBehavior.Restrict); b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class SharingTagConfiguration : IEntityTypeConfiguration<SharingTag>
{
    public void Configure(EntityTypeBuilder<SharingTag> b) { b.ToTable("SharingTags"); b.HasKey(x => x.Id); b.Property(x => x.Name).HasMaxLength(100).IsRequired(); b.Property(x => x.Slug).HasMaxLength(100).IsRequired(); b.Property(x => x.Color).HasMaxLength(30); b.Property(x => x.IsActive).HasDefaultValue(true).IsRequired(); b.HasIndex(x => x.Slug).IsUnique(); }
}

public sealed class SharingContentTagConfiguration : IEntityTypeConfiguration<SharingContentTag>
{
    public void Configure(EntityTypeBuilder<SharingContentTag> b) { b.ToTable("SharingContentTags"); b.HasKey(x => new { x.SharingContentId, x.SharingTagId }); b.HasOne(x => x.Content).WithMany(x => x.Tags).HasForeignKey(x => x.SharingContentId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.Tag).WithMany(x => x.Contents).HasForeignKey(x => x.SharingTagId).OnDelete(DeleteBehavior.Restrict); }
}

public sealed class SharingScheduleConfiguration : IEntityTypeConfiguration<SharingSchedule>
{
    public void Configure(EntityTypeBuilder<SharingSchedule> b)
    {
        b.ToTable("SharingSchedules", t => { t.HasCheckConstraint("CK_SharingSchedules_Times", "\"EndsAtUtc\" > \"StartsAtUtc\""); t.HasCheckConstraint("CK_SharingSchedules_Status", "\"Status\" IN (0,1,2,3,4)"); t.HasCheckConstraint("CK_SharingSchedules_DeliveryMode", "(\"DeliveryMode\" = 0 AND \"MeetingUrl\" IS NOT NULL) OR (\"DeliveryMode\" = 1 AND \"Location\" IS NOT NULL) OR (\"DeliveryMode\" = 2 AND \"MeetingUrl\" IS NOT NULL AND \"Location\" IS NOT NULL)"); });
        b.HasKey(x => x.Id); b.Property(x => x.Title).HasMaxLength(200).IsRequired(); b.Property(x => x.Description).HasColumnType("text"); b.Property(x => x.SharingType).HasConversion<int>().IsRequired(); b.Property(x => x.DeliveryMode).HasConversion<int>().IsRequired(); b.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired(); b.Property(x => x.Location).HasMaxLength(500); b.Property(x => x.MeetingUrl).HasMaxLength(2048); b.Property(x => x.Status).HasConversion<int>().IsRequired(); b.Property(x => x.AudienceScope).HasConversion<int>().IsRequired(); b.Property(x => x.CancellationReason).HasMaxLength(4000); b.Property(x => x.Version).IsConcurrencyToken().IsRequired(); b.HasIndex(x => new { x.Status, x.StartsAtUtc }); b.HasIndex(x => new { x.StartsAtUtc, x.EndsAtUtc }); b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class SharingSchedulePresenterConfiguration : IEntityTypeConfiguration<SharingSchedulePresenter>
{
    public void Configure(EntityTypeBuilder<SharingSchedulePresenter> b) { b.ToTable("SharingSchedulePresenters"); b.HasKey(x => x.Id); b.Property(x => x.PresenterRole).HasConversion<int>().IsRequired(); b.Property(x => x.SortOrder).IsRequired(); b.HasIndex(x => new { x.SharingScheduleId, x.UserId, x.PresenterRole }).IsUnique(); b.HasIndex(x => new { x.UserId, x.SharingScheduleId }); b.HasOne(x => x.Schedule).WithMany(x => x.Presenters).HasForeignKey(x => x.SharingScheduleId).OnDelete(DeleteBehavior.Restrict); b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); }
}

public sealed class SharingScheduleContentConfiguration : IEntityTypeConfiguration<SharingScheduleContent>
{
    public void Configure(EntityTypeBuilder<SharingScheduleContent> b) { b.ToTable("SharingScheduleContents"); b.HasKey(x => new { x.SharingScheduleId, x.SharingContentId }); b.HasIndex(x => new { x.SharingScheduleId, x.SortOrder }); b.HasOne(x => x.Schedule).WithMany(x => x.Contents).HasForeignKey(x => x.SharingScheduleId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.Content).WithMany(x => x.Schedules).HasForeignKey(x => x.SharingContentId).OnDelete(DeleteBehavior.Restrict); }
}

public sealed class SharingScheduleAudienceGenerationConfiguration : IEntityTypeConfiguration<SharingScheduleAudienceGeneration>
{
    public void Configure(EntityTypeBuilder<SharingScheduleAudienceGeneration> b) { b.ToTable("SharingScheduleAudienceGenerations"); b.HasKey(x => new { x.SharingScheduleId, x.ClubGenerationId }); b.HasOne(x => x.Schedule).WithMany(x => x.AudienceGenerations).HasForeignKey(x => x.SharingScheduleId).OnDelete(DeleteBehavior.Restrict); b.HasOne<GdscSharingPlatform.Domain.Memberships.ClubGeneration>().WithMany().HasForeignKey(x => x.ClubGenerationId).OnDelete(DeleteBehavior.Restrict); }
}

public sealed class SharingScheduleAudienceDepartmentConfiguration : IEntityTypeConfiguration<SharingScheduleAudienceDepartment>
{
    public void Configure(EntityTypeBuilder<SharingScheduleAudienceDepartment> b) { b.ToTable("SharingScheduleAudienceDepartments"); b.HasKey(x => new { x.SharingScheduleId, x.DepartmentId }); b.HasOne(x => x.Schedule).WithMany(x => x.AudienceDepartments).HasForeignKey(x => x.SharingScheduleId).OnDelete(DeleteBehavior.Restrict); b.HasOne<GdscSharingPlatform.Domain.Departments.Department>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict); }
}
