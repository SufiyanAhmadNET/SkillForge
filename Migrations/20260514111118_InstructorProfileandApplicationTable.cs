using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillForge.Migrations
{
    /// <inheritdoc />
    public partial class InstructorProfileandApplicationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Profession",
                table: "instructorProfiles",
                newName: "Expertise");

            migrationBuilder.RenameColumn(
                name: "Bio",
                table: "instructorProfiles",
                newName: "CurrentRole");

            migrationBuilder.AddColumn<string>(
                name: "AboutYou",
                table: "instructorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsExperience",
                table: "instructorProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MentorApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstructorId = table.Column<int>(type: "int", nullable: false),
                    ResumePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhyTeach = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Topics = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PortfolioUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdminComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorApplications_instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MentorApplications_InstructorId",
                table: "MentorApplications",
                column: "InstructorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentorApplications");

            migrationBuilder.DropColumn(
                name: "AboutYou",
                table: "instructorProfiles");

            migrationBuilder.DropColumn(
                name: "YearsExperience",
                table: "instructorProfiles");

            migrationBuilder.RenameColumn(
                name: "Expertise",
                table: "instructorProfiles",
                newName: "Profession");

            migrationBuilder.RenameColumn(
                name: "CurrentRole",
                table: "instructorProfiles",
                newName: "Bio");
        }
    }
}
