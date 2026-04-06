using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillForge.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VerificationToken",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "VerificationToken",
                table: "Students");
        }
    }
}
