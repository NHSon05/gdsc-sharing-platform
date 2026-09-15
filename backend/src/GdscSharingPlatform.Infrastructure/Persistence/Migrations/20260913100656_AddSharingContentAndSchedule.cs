using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdscSharingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSharingContentAndSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharingContents",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BodyMarkdown = table.Column<string>(type: "text", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewNote = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingContents", x => x.Id);
                    table.CheckConstraint("CK_SharingContents_Status", "\"Status\" IN (0,1,2,3,4)");
                    table.CheckConstraint("CK_SharingContents_Version", "\"Version\" >= 0");
                    table.ForeignKey(
                        name: "FK_SharingContents_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingContents_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharingSchedules",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SharingType = table.Column<int>(type: "integer", nullable: false),
                    DeliveryMode = table.Column<int>(type: "integer", nullable: false),
                    StartsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MeetingUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AudienceScope = table.Column<int>(type: "integer", nullable: false),
                    CancellationReason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingSchedules", x => x.Id);
                    table.CheckConstraint("CK_SharingSchedules_DeliveryMode", "(\"DeliveryMode\" = 0 AND \"MeetingUrl\" IS NOT NULL) OR (\"DeliveryMode\" = 1 AND \"Location\" IS NOT NULL) OR (\"DeliveryMode\" = 2 AND \"MeetingUrl\" IS NOT NULL AND \"Location\" IS NOT NULL)");
                    table.CheckConstraint("CK_SharingSchedules_Status", "\"Status\" IN (0,1,2,3,4)");
                    table.CheckConstraint("CK_SharingSchedules_Times", "\"EndsAtUtc\" > \"StartsAtUtc\"");
                    table.ForeignKey(
                        name: "FK_SharingSchedules_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharingTags",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharingContentAuthors",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorRole = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingContentAuthors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharingContentAuthors_SharingContents_SharingContentId",
                        column: x => x.SharingContentId,
                        principalSchema: "gdsc",
                        principalTable: "SharingContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingContentAuthors_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharingResources",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResourceType = table.Column<int>(type: "integer", nullable: false),
                    ExternalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingResources", x => x.Id);
                    table.CheckConstraint("CK_SharingResources_SortOrder", "\"SortOrder\" >= 0");
                    table.ForeignKey(
                        name: "FK_SharingResources_SharingContents_SharingContentId",
                        column: x => x.SharingContentId,
                        principalSchema: "gdsc",
                        principalTable: "SharingContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingResources_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharingScheduleAudienceDepartments",
                schema: "gdsc",
                columns: table => new
                {
                    SharingScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingScheduleAudienceDepartments", x => new { x.SharingScheduleId, x.DepartmentId });
                    table.ForeignKey(
                        name: "FK_SharingScheduleAudienceDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "gdsc",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingScheduleAudienceDepartments_SharingSchedules_Sharing~",
                        column: x => x.SharingScheduleId,
                        principalSchema: "gdsc",
                        principalTable: "SharingSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharingScheduleAudienceGenerations",
                schema: "gdsc",
                columns: table => new
                {
                    SharingScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClubGenerationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingScheduleAudienceGenerations", x => new { x.SharingScheduleId, x.ClubGenerationId });
                    table.ForeignKey(
                        name: "FK_SharingScheduleAudienceGenerations_ClubGenerations_ClubGene~",
                        column: x => x.ClubGenerationId,
                        principalSchema: "gdsc",
                        principalTable: "ClubGenerations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingScheduleAudienceGenerations_SharingSchedules_Sharing~",
                        column: x => x.SharingScheduleId,
                        principalSchema: "gdsc",
                        principalTable: "SharingSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharingScheduleContents",
                schema: "gdsc",
                columns: table => new
                {
                    SharingScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingScheduleContents", x => new { x.SharingScheduleId, x.SharingContentId });
                    table.ForeignKey(
                        name: "FK_SharingScheduleContents_SharingContents_SharingContentId",
                        column: x => x.SharingContentId,
                        principalSchema: "gdsc",
                        principalTable: "SharingContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingScheduleContents_SharingSchedules_SharingScheduleId",
                        column: x => x.SharingScheduleId,
                        principalSchema: "gdsc",
                        principalTable: "SharingSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharingSchedulePresenters",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PresenterRole = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingSchedulePresenters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharingSchedulePresenters_SharingSchedules_SharingScheduleId",
                        column: x => x.SharingScheduleId,
                        principalSchema: "gdsc",
                        principalTable: "SharingSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingSchedulePresenters_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharingContentTags",
                schema: "gdsc",
                columns: table => new
                {
                    SharingContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingTagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingContentTags", x => new { x.SharingContentId, x.SharingTagId });
                    table.ForeignKey(
                        name: "FK_SharingContentTags_SharingContents_SharingContentId",
                        column: x => x.SharingContentId,
                        principalSchema: "gdsc",
                        principalTable: "SharingContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharingContentTags_SharingTags_SharingTagId",
                        column: x => x.SharingTagId,
                        principalSchema: "gdsc",
                        principalTable: "SharingTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharingContentAuthors_SharingContentId",
                schema: "gdsc",
                table: "SharingContentAuthors",
                column: "SharingContentId",
                unique: true,
                filter: "\"AuthorRole\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SharingContentAuthors_SharingContentId_UserId",
                schema: "gdsc",
                table: "SharingContentAuthors",
                columns: new[] { "SharingContentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharingContentAuthors_UserId_SharingContentId",
                schema: "gdsc",
                table: "SharingContentAuthors",
                columns: new[] { "UserId", "SharingContentId" });

            migrationBuilder.CreateIndex(
                name: "IX_SharingContents_CreatedByUserId",
                schema: "gdsc",
                table: "SharingContents",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingContents_ReviewedByUserId",
                schema: "gdsc",
                table: "SharingContents",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingContents_Slug",
                schema: "gdsc",
                table: "SharingContents",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharingContents_Status_PublishedAtUtc",
                schema: "gdsc",
                table: "SharingContents",
                columns: new[] { "Status", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SharingContentTags_SharingTagId",
                schema: "gdsc",
                table: "SharingContentTags",
                column: "SharingTagId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingResources_CreatedByUserId",
                schema: "gdsc",
                table: "SharingResources",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingResources_SharingContentId_IsActive_SortOrder",
                schema: "gdsc",
                table: "SharingResources",
                columns: new[] { "SharingContentId", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SharingScheduleAudienceDepartments_DepartmentId",
                schema: "gdsc",
                table: "SharingScheduleAudienceDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingScheduleAudienceGenerations_ClubGenerationId",
                schema: "gdsc",
                table: "SharingScheduleAudienceGenerations",
                column: "ClubGenerationId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingScheduleContents_SharingContentId",
                schema: "gdsc",
                table: "SharingScheduleContents",
                column: "SharingContentId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingScheduleContents_SharingScheduleId_SortOrder",
                schema: "gdsc",
                table: "SharingScheduleContents",
                columns: new[] { "SharingScheduleId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SharingSchedulePresenters_SharingScheduleId_UserId_Presente~",
                schema: "gdsc",
                table: "SharingSchedulePresenters",
                columns: new[] { "SharingScheduleId", "UserId", "PresenterRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharingSchedulePresenters_UserId_SharingScheduleId",
                schema: "gdsc",
                table: "SharingSchedulePresenters",
                columns: new[] { "UserId", "SharingScheduleId" });

            migrationBuilder.CreateIndex(
                name: "IX_SharingSchedules_CreatedByUserId",
                schema: "gdsc",
                table: "SharingSchedules",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingSchedules_StartsAtUtc_EndsAtUtc",
                schema: "gdsc",
                table: "SharingSchedules",
                columns: new[] { "StartsAtUtc", "EndsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SharingSchedules_Status_StartsAtUtc",
                schema: "gdsc",
                table: "SharingSchedules",
                columns: new[] { "Status", "StartsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SharingTags_Slug",
                schema: "gdsc",
                table: "SharingTags",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharingContentAuthors",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "SharingContentTags",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "SharingResources",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "SharingScheduleAudienceDepartments",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "SharingScheduleAudienceGenerations",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "SharingScheduleContents",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "SharingSchedulePresenters",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "SharingTags",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "SharingContents",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "SharingSchedules",
                schema: "gdsc");
        }
    }
}
