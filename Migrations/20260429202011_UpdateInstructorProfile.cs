using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillForge.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInstructorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GithubUrl",
                table: "instructorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Headline",
                table: "instructorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedinUrl",
                table: "instructorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "instructorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterUrl",
                table: "instructorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "instructorProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GithubUrl",
                table: "instructorProfiles");

            migrationBuilder.DropColumn(
                name: "Headline",
                table: "instructorProfiles");

            migrationBuilder.DropColumn(
                name: "LinkedinUrl",
                table: "instructorProfiles");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "instructorProfiles");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "instructorProfiles");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "instructorProfiles");
        }
    }
}
