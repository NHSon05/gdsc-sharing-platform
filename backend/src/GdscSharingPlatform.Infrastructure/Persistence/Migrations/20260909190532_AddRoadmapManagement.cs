using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdscSharingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadmapManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoadmapCategories",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Icon = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapCategories", x => x.Id);
                    table.CheckConstraint("CK_RoadmapCategories_Name", "btrim(\"Name\") <> ''");
                    table.CheckConstraint("CK_RoadmapCategories_Slug", "\"Slug\" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                    table.CheckConstraint("CK_RoadmapCategories_SortOrder", "\"SortOrder\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Roadmaps",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    EstimatedDuration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Prerequisites = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roadmaps", x => x.Id);
                    table.CheckConstraint("CK_Roadmaps_Level", "\"Level\" IN (0, 1, 2, 3)");
                    table.CheckConstraint("CK_Roadmaps_ShortDescription", "btrim(\"ShortDescription\") <> ''");
                    table.CheckConstraint("CK_Roadmaps_Slug", "\"Slug\" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                    table.CheckConstraint("CK_Roadmaps_SortOrder", "\"SortOrder\" >= 0");
                    table.CheckConstraint("CK_Roadmaps_Status", "\"Status\" IN (0, 1, 2)");
                    table.CheckConstraint("CK_Roadmaps_Title", "btrim(\"Title\") <> ''");
                    table.ForeignKey(
                        name: "FK_Roadmaps_RoadmapCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "gdsc",
                        principalTable: "RoadmapCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Roadmaps_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Roadmaps_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapNodes",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoadmapId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LearningObjectives = table.Column<string>(type: "text", nullable: true),
                    EstimatedDuration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NodeType = table.Column<int>(type: "integer", nullable: false),
                    PositionX = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PositionY = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Width = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Icon = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapNodes", x => x.Id);
                    table.UniqueConstraint("AK_RoadmapNodes_RoadmapId_Id", x => new { x.RoadmapId, x.Id });
                    table.CheckConstraint("CK_RoadmapNodes_NodeType", "\"NodeType\" IN (0, 1, 2)");
                    table.CheckConstraint("CK_RoadmapNodes_Slug", "\"Slug\" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                    table.CheckConstraint("CK_RoadmapNodes_SortOrder", "\"SortOrder\" >= 0");
                    table.CheckConstraint("CK_RoadmapNodes_Title", "btrim(\"Title\") <> ''");
                    table.CheckConstraint("CK_RoadmapNodes_Width", "\"Width\" IS NULL OR \"Width\" > 0");
                    table.ForeignKey(
                        name: "FK_RoadmapNodes_Roadmaps_RoadmapId",
                        column: x => x.RoadmapId,
                        principalSchema: "gdsc",
                        principalTable: "Roadmaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearningResources",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoadmapNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResourceType = table.Column<int>(type: "integer", nullable: false),
                    ExternalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StoredFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningResources", x => x.Id);
                    table.CheckConstraint("CK_LearningResources_ResourceMetadata", "(\"ResourceType\" = 0 AND \"ExternalUrl\" IS NOT NULL AND btrim(\"ExternalUrl\") <> '' AND \"OriginalFileName\" IS NULL AND \"StoredFileName\" IS NULL AND \"StorageKey\" IS NULL AND \"FileSize\" IS NULL AND \"ContentType\" IS NULL) OR (\"ResourceType\" = 1 AND \"ExternalUrl\" IS NULL AND \"OriginalFileName\" IS NOT NULL AND btrim(\"OriginalFileName\") <> '' AND \"StoredFileName\" IS NOT NULL AND btrim(\"StoredFileName\") <> '' AND \"StorageKey\" IS NOT NULL AND btrim(\"StorageKey\") <> '' AND \"FileSize\" IS NOT NULL AND \"FileSize\" >= 0 AND \"ContentType\" IS NOT NULL AND btrim(\"ContentType\") <> '')");
                    table.CheckConstraint("CK_LearningResources_SortOrder", "\"SortOrder\" >= 0");
                    table.CheckConstraint("CK_LearningResources_Title", "btrim(\"Title\") <> ''");
                    table.ForeignKey(
                        name: "FK_LearningResources_RoadmapNodes_RoadmapNodeId",
                        column: x => x.RoadmapNodeId,
                        principalSchema: "gdsc",
                        principalTable: "RoadmapNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningResources_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "gdsc",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapEdges",
                schema: "gdsc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoadmapId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LineStyle = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapEdges", x => x.Id);
                    table.CheckConstraint("CK_RoadmapEdges_LineStyle", "\"LineStyle\" IN (0, 1)");
                    table.CheckConstraint("CK_RoadmapEdges_NoSelfLoop", "\"SourceNodeId\" <> \"TargetNodeId\"");
                    table.CheckConstraint("CK_RoadmapEdges_RelationType", "\"RelationType\" IN (0, 1, 2)");
                    table.CheckConstraint("CK_RoadmapEdges_RequiredSolid", "\"RelationType\" <> 0 OR \"LineStyle\" = 0");
                    table.CheckConstraint("CK_RoadmapEdges_SortOrder", "\"SortOrder\" >= 0");
                    table.ForeignKey(
                        name: "FK_RoadmapEdges_RoadmapNodes_RoadmapId_SourceNodeId",
                        columns: x => new { x.RoadmapId, x.SourceNodeId },
                        principalSchema: "gdsc",
                        principalTable: "RoadmapNodes",
                        principalColumns: new[] { "RoadmapId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoadmapEdges_RoadmapNodes_RoadmapId_TargetNodeId",
                        columns: x => new { x.RoadmapId, x.TargetNodeId },
                        principalSchema: "gdsc",
                        principalTable: "RoadmapNodes",
                        principalColumns: new[] { "RoadmapId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoadmapEdges_Roadmaps_RoadmapId",
                        column: x => x.RoadmapId,
                        principalSchema: "gdsc",
                        principalTable: "Roadmaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningResources_CreatedByUserId",
                schema: "gdsc",
                table: "LearningResources",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningResources_RoadmapNodeId_IsActive_SortOrder",
                schema: "gdsc",
                table: "LearningResources",
                columns: new[] { "RoadmapNodeId", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapCategories_Slug",
                schema: "gdsc",
                table: "RoadmapCategories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapEdges_RoadmapId_IsActive",
                schema: "gdsc",
                table: "RoadmapEdges",
                columns: new[] { "RoadmapId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapEdges_RoadmapId_SourceNodeId_TargetNodeId_RelationTy~",
                schema: "gdsc",
                table: "RoadmapEdges",
                columns: new[] { "RoadmapId", "SourceNodeId", "TargetNodeId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapEdges_RoadmapId_TargetNodeId",
                schema: "gdsc",
                table: "RoadmapEdges",
                columns: new[] { "RoadmapId", "TargetNodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapNodes_RoadmapId_IsActive",
                schema: "gdsc",
                table: "RoadmapNodes",
                columns: new[] { "RoadmapId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapNodes_RoadmapId_Slug",
                schema: "gdsc",
                table: "RoadmapNodes",
                columns: new[] { "RoadmapId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_CategoryId_Status",
                schema: "gdsc",
                table: "Roadmaps",
                columns: new[] { "CategoryId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_CreatedByUserId",
                schema: "gdsc",
                table: "Roadmaps",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_Slug",
                schema: "gdsc",
                table: "Roadmaps",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_Status_SortOrder",
                schema: "gdsc",
                table: "Roadmaps",
                columns: new[] { "Status", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_UpdatedByUserId",
                schema: "gdsc",
                table: "Roadmaps",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningResources",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "RoadmapEdges",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "RoadmapNodes",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "Roadmaps",
                schema: "gdsc");

            migrationBuilder.DropTable(
                name: "RoadmapCategories",
                schema: "gdsc");
        }
    }
}
