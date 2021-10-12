using Microsoft.EntityFrameworkCore.Migrations;

namespace advent_of_qode_server.Migrations
{
    public partial class add_cascading_dependency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Option_Questions_QuestionId",
                table: "Option");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionId",
                table: "Option",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Option_Questions_QuestionId",
                table: "Option",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Option_Questions_QuestionId",
                table: "Option");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionId",
                table: "Option",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Option_Questions_QuestionId",
                table: "Option",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
