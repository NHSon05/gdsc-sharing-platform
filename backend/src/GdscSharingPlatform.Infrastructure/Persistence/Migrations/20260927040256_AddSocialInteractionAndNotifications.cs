using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdscSharingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialInteractionAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentComments",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    BodyMarkdown = table.Column<string>(type: "text", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ModerationReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModeratedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentComments", x => x.Id);
                    table.CheckConstraint("CK_ContentComments_Status", "\"Status\" IN (0,1,2)");
                    table.CheckConstraint("CK_ContentComments_Version", "\"Version\" >= 0");
                    table.ForeignKey(
                        name: "FK_ContentComments_ContentComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalSchema: "gdsc",
                        principalTable: "ContentComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentComments_SharingContents_SharingContentId",
                        column: x => x.SharingContentId,
                        principalSchema: "gdsc",
                        principalTable: "SharingContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentComments_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentComments_Users_ModeratedByUserId",
                        column: x => x.ModeratedByUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContentLikes",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentLikes_SharingContents_SharingContentId",
                        column: x => x.SharingContentId,
                        principalSchema: "gdsc",
                        principalTable: "SharingContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentLikes_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Route = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DeadLetteredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedContents",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedContents_SharingContents_SharingContentId",
                        column: x => x.SharingContentId,
                        principalSchema: "gdsc",
                        principalTable: "SharingContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavedContents_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleRsvps",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RespondedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleRsvps", x => x.Id);
                    table.CheckConstraint("CK_ScheduleRsvps_Status", "\"Status\" IN (0,1,2)");
                    table.CheckConstraint("CK_ScheduleRsvps_Version", "\"Version\" >= 0");
                    table.ForeignKey(
                        name: "FK_ScheduleRsvps_SharingSchedules_SharingScheduleId",
                        column: x => x.SharingScheduleId,
                        principalSchema: "gdsc",
                        principalTable: "SharingSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleRsvps_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_AuthorUserId_CreatedAtUtc",
                schema: "gdsc",
                table: "ContentComments",
                columns: new[] { "AuthorUserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_ModeratedByUserId",
                schema: "gdsc",
                table: "ContentComments",
                column: "ModeratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_ParentCommentId_CreatedAtUtc",
                schema: "gdsc",
                table: "ContentComments",
                columns: new[] { "ParentCommentId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentComments_SharingContentId_Status_CreatedAtUtc",
                schema: "gdsc",
                table: "ContentComments",
                columns: new[] { "SharingContentId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentLikes_SharingContentId_UserId",
                schema: "gdsc",
                table: "ContentLikes",
                columns: new[] { "SharingContentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentLikes_UserId_CreatedAtUtc",
                schema: "gdsc",
                table: "ContentLikes",
                columns: new[] { "UserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ActorUserId",
                schema: "gdsc",
                table: "Notifications",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_CreatedAtUtc",
                schema: "gdsc",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_EventId",
                schema: "gdsc",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_IsRead_CreatedAtUtc",
                schema: "gdsc",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "IsRead", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type_ActorUserId_EntityId_RecipientUserId_Cre~",
                schema: "gdsc",
                table: "Notifications",
                columns: new[] { "Type", "ActorUserId", "EntityId", "RecipientUserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAtUtc_NextAttemptAtUtc_OccurredAtUtc",
                schema: "gdsc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedAtUtc", "NextAttemptAtUtc", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SavedContents_SharingContentId_UserId",
                schema: "gdsc",
                table: "SavedContents",
                columns: new[] { "SharingContentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedContents_UserId_CreatedAtUtc",
                schema: "gdsc",
                table: "SavedContents",
                columns: new[] { "UserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleRsvps_SharingScheduleId_Status",
                schema: "gdsc",
                table: "ScheduleRsvps",
                columns: new[] { "SharingScheduleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleRsvps_SharingScheduleId_UserId",
                schema: "gdsc",
                table: "ScheduleRsvps",
                columns: new[] { "SharingScheduleId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleRsvps_UserId",
                schema: "gdsc",
                table: "ScheduleRsvps",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentComments",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "ContentLikes",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "SavedContents",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "ScheduleRsvps",
                schema: "gdsc");
        }
    }
}
