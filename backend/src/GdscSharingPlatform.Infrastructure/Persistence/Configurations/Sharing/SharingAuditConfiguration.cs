using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Sharing;

public sealed class SharingAuditConfiguration : IEntityTypeConfiguration<SharingAuditEntry>
{
    public void Configure(EntityTypeBuilder<SharingAuditEntry> b)
    {
        b.ToTable("SharingAuditEntries");
        b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasMaxLength(80).IsRequired();
        b.Property(x => x.Entity).HasMaxLength(80).IsRequired();
        b.Property(x => x.TraceId).HasMaxLength(128).IsRequired();
        b.Property(x => x.Metadata).HasColumnType("jsonb").IsRequired();
        b.HasIndex(x => new { x.Entity, x.EntityId, x.TimestampUtc });
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
