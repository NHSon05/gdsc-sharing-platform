using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GdscSharingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLastLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Generation",
                schema: "gdsc",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentCode",
                schema: "gdsc",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenVersion",
                schema: "gdsc",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Generation",
                schema: "gdsc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StudentCode",
                schema: "gdsc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TokenVersion",
                schema: "gdsc",
                table: "Users");
        }
    }
}
