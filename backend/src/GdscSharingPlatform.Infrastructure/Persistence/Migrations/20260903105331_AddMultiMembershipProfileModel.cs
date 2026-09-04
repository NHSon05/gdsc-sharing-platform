using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdscSharingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiMembershipProfileModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StudentCode",
                schema: "gdsc",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                schema: "gdsc",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubUrl",
                schema: "gdsc",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "gdsc",
                table: "Departments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "gdsc",
                table: "Departments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                schema: "gdsc",
                table: "Departments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                schema: "gdsc",
                table: "Departments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                schema: "gdsc",
                table: "Departments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE gdsc.""Departments""
                SET ""Slug"" = LOWER(REPLACE(REPLACE(""Code"", '&', 'and'), ' ', '-'))
                WHERE ""Slug"" = '' OR ""Slug"" IS NULL;
            ");

            migrationBuilder.CreateTable(
                name: "ClubGenerations",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubGenerations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubRoles",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubMemberships",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenerationId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    LeftAt = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubMemberships_ClubGenerations_GenerationId",
                        column: x => x.GenerationId,
                        principalSchema: "gdsc",
                        principalTable: "ClubGenerations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentMemberships",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClubMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    JoinedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    LeftAt = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentMemberships_ClubMemberships_ClubMembershipId",
                        column: x => x.ClubMembershipId,
                        principalSchema: "gdsc",
                        principalTable: "ClubMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentMemberships_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "gdsc",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleAssignments",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClubRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EndedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_ClubRoles_ClubRoleId",
                        column: x => x.ClubRoleId,
                        principalSchema: "gdsc",
                        principalTable: "ClubRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_DepartmentMemberships_DepartmentMembershipId",
                        column: x => x.DepartmentMembershipId,
                        principalSchema: "gdsc",
                        principalTable: "DepartmentMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_StudentCode",
                schema: "gdsc",
                table: "Users",
                column: "StudentCode",
                unique: true,
                filter: "\"StudentCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Slug",
                schema: "gdsc",
                table: "Departments",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubGenerations_IsActive",
                schema: "gdsc",
                table: "ClubGenerations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ClubGenerations_Name",
                schema: "gdsc",
                table: "ClubGenerations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ClubGenerations_Number",
                schema: "gdsc",
                table: "ClubGenerations",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_GenerationId",
                schema: "gdsc",
                table: "ClubMemberships",
                column: "GenerationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_IsActive",
                schema: "gdsc",
                table: "ClubMemberships",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_UserId",
                schema: "gdsc",
                table: "ClubMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_UserId_GenerationId",
                schema: "gdsc",
                table: "ClubMemberships",
                columns: new[] { "UserId", "GenerationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubRoles_Code",
                schema: "gdsc",
                table: "ClubRoles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubRoles_IsActive",
                schema: "gdsc",
                table: "ClubRoles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ClubRoles_Level",
                schema: "gdsc",
                table: "ClubRoles",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMemberships_ClubMembershipId",
                schema: "gdsc",
                table: "DepartmentMemberships",
                column: "ClubMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMemberships_ClubMembershipId_DepartmentId",
                schema: "gdsc",
                table: "DepartmentMemberships",
                columns: new[] { "ClubMembershipId", "DepartmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMemberships_DepartmentId",
                schema: "gdsc",
                table: "DepartmentMemberships",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMemberships_IsActive",
                schema: "gdsc",
                table: "DepartmentMemberships",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMemberships_IsPrimary",
                schema: "gdsc",
                table: "DepartmentMemberships",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_AssignedByUserId",
                schema: "gdsc",
                table: "RoleAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_ClubRoleId",
                schema: "gdsc",
                table: "RoleAssignments",
                column: "ClubRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_DepartmentMembershipId",
                schema: "gdsc",
                table: "RoleAssignments",
                column: "DepartmentMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_DepartmentMembershipId_ClubRoleId",
                schema: "gdsc",
                table: "RoleAssignments",
                columns: new[] { "DepartmentMembershipId", "ClubRoleId" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_IsActive",
                schema: "gdsc",
                table: "RoleAssignments",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleAssignments",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "ClubRoles",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "DepartmentMemberships",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "ClubMemberships",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "ClubGenerations",
                schema: "gdsc");

            migrationBuilder.DropIndex(
                name: "IX_Users_StudentCode",
                schema: "gdsc",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Slug",
                schema: "gdsc",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "GitHubUrl",
                schema: "gdsc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Color",
                schema: "gdsc",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Icon",
                schema: "gdsc",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Slug",
                schema: "gdsc",
                table: "Departments");

            migrationBuilder.AlterColumn<string>(
                name: "StudentCode",
                schema: "gdsc",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                schema: "gdsc",
                table: "Users",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "gdsc",
                table: "Departments",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "gdsc",
                table: "Departments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
