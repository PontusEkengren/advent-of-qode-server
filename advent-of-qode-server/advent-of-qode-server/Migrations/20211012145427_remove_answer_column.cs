using Microsoft.EntityFrameworkCore.Migrations;

namespace advent_of_qode_server.Migrations
{
    public partial class remove_answer_column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer",
                table: "Questions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "Questions",
                type: "text",
                nullable: true);
        }
    }
}
