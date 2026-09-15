using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdscSharingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSharingAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharingAuditEntries",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Entity = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimestampUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TraceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharingAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharingAuditEntries_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharingAuditEntries_ActorUserId",
                schema: "gdsc",
                table: "SharingAuditEntries",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharingAuditEntries_Entity_EntityId_TimestampUtc",
                schema: "gdsc",
                table: "SharingAuditEntries",
                columns: new[] { "Entity", "EntityId", "TimestampUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharingAuditEntries",
                schema: "gdsc");
        }
    }
}
