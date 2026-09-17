using GdscSharingPlatform.Domain.Interviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Interviews;

public sealed class InterviewQuestionConfiguration : IEntityTypeConfiguration<InterviewQuestion>
{
    public void Configure(EntityTypeBuilder<InterviewQuestion> b)
    {
        b.ToTable("InterviewQuestions", t =>
        {
            t.HasCheckConstraint("InterviewQuestions_Data_check", "jsonb_typeof(\"Data\") = 'object'");
            t.HasCheckConstraint("InterviewQuestions_Status_check", "\"Status\" IN ('draft', 'published')");
            t.HasCheckConstraint("InterviewQuestions_check", "\"Data\"->>'id' IS NOT NULL AND \"Id\" = (\"Data\"->>'id')::uuid");
        });
        b.HasKey(x => x.Id).HasName("InterviewQuestions_pkey");
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Data).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.Question).HasComputedColumnSql("\"Data\"->>'question'", stored: true).IsRequired();
        b.Property(x => x.Slug).HasComputedColumnSql("\"Data\"->>'slug'", stored: true).IsRequired();
        b.Property(x => x.Level).HasComputedColumnSql("\"Data\"->>'level'", stored: true).IsRequired();
        b.Property(x => x.Access).HasComputedColumnSql("\"Data\"->>'access'", stored: true).IsRequired();
        b.Property(x => x.NeedsReview).HasComputedColumnSql("(\"Data\"->>'needsReview')::boolean", stored: true);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.Status).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("InterviewQuestions_Slug_key");
        b.HasIndex(x => x.Level);
    }
}
