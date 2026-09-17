using Microsoft.EntityFrameworkCore.Migrations;

namespace GdscSharingPlatform.Infrastructure.Persistence.Migrations;

public partial class AddInterviewQuestions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Adopt the pre-seeded table without overwriting records.
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS gdsc."InterviewQuestions" (
              "Id" uuid PRIMARY KEY,
              "Data" jsonb NOT NULL CHECK (jsonb_typeof("Data") = 'object'),
              "Question" text GENERATED ALWAYS AS ("Data"->>'question') STORED NOT NULL,
              "Slug" text GENERATED ALWAYS AS ("Data"->>'slug') STORED NOT NULL UNIQUE,
              "Level" text GENERATED ALWAYS AS ("Data"->>'level') STORED NOT NULL,
              "Access" text GENERATED ALWAYS AS ("Data"->>'access') STORED NOT NULL,
              "NeedsReview" boolean GENERATED ALWAYS AS (("Data"->>'needsReview')::boolean) STORED NOT NULL,
              "Status" text NOT NULL CHECK ("Status" IN ('draft', 'published')),
              "CreatedAt" timestamptz NOT NULL DEFAULT now(),
              CHECK ("Data"->>'id' IS NOT NULL AND "Id" = ("Data"->>'id')::uuid)
            );
            CREATE INDEX IF NOT EXISTS "IX_InterviewQuestions_Level" ON gdsc."InterviewQuestions" ("Level");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The table may predate EF ownership. Retain it and its records on downgrade;
        // Up can safely adopt it again. Physical removal is a separate manual operation.
    }
}
