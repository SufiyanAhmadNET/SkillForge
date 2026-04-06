using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillForge.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleIdToInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "instructors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "instructors");
        }
    }
}
