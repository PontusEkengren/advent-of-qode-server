using Microsoft.EntityFrameworkCore.Migrations;

namespace advent_of_qode_server.Migrations
{
    public partial class add_question_options : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answers",
                table: "Questions");

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "Questions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "Questions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Options",
                table: "Questions");

            migrationBuilder.AddColumn<string>(
                name: "Answers",
                table: "Questions",
                type: "text",
                nullable: true);
        }
    }
}
