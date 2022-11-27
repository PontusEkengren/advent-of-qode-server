using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace adventofqodeserver.Migrations
{
    /// <inheritdoc />
    public partial class addquercounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionSeen",
                table: "StartTime",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionSeen",
                table: "StartTime");
        }
    }
}
