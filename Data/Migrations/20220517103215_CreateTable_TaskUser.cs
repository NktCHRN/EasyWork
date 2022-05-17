using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class CreateTable_TaskUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_AspNetUsers_ExecutorId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ExecutorId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ExecutorId",
                table: "Tasks");

            migrationBuilder.CreateTable(
                name: "TaskUser",
                columns: table => new
                {
                    ExecutorsId = table.Column<int>(type: "int", nullable: false),
                    TasksId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskUser", x => new { x.ExecutorsId, x.TasksId });
                    table.ForeignKey(
                        name: "FK_TaskUser_AspNetUsers_ExecutorsId",
                        column: x => x.ExecutorsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskUser_Tasks_TasksId",
                        column: x => x.TasksId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskUser_TasksId",
                table: "TaskUser",
                column: "TasksId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskUser");

            migrationBuilder.AddColumn<int>(
                name: "ExecutorId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ExecutorId",
                table: "Tasks",
                column: "ExecutorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_ExecutorId",
                table: "Tasks",
                column: "ExecutorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
