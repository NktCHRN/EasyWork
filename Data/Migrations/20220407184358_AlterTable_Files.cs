using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class AlterTable_Files : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Messages_MessageId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_MessageId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "Files");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "Files",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "Files",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MessageId",
                table: "Files",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_MessageId",
                table: "Files",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Messages_MessageId",
                table: "Files",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id");
        }
    }
}
