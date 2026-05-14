using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillForge.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "StudentProfiles");

            migrationBuilder.RenameColumn(
                name: "Profession",
                table: "StudentProfiles",
                newName: "Interests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Interests",
                table: "StudentProfiles",
                newName: "Profession");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
