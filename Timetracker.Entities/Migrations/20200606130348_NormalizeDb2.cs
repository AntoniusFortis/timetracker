using Microsoft.EntityFrameworkCore.Migrations;

namespace Timetracker.Entities.Migrations
{
    public partial class NormalizeDb2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql( @"INSERT INTO [dbo].[Rights] ([Title]) VALUES (N'Создатель')" );

            migrationBuilder.Sql( @"
				UPDATE [dbo].[States]
				SET [Title] = N'Новая'
				WHERE [Id] = 1

				UPDATE [dbo].[States]
				SET [Title] = N'Рассмотрена'
				WHERE [Id] = 2

				UPDATE [dbo].[States]
				SET [Title] = N'Исполняется'
				WHERE [Id] = 3

				UPDATE [dbo].[States]
				SET [Title] = N'Отложена'
				WHERE [Id] = 4
            " );

            migrationBuilder.Sql( @"INSERT INTO [dbo].[States] ([Title]) VALUES (N'Выполнена'), (N'Закрыта')" );

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Projects_ProjectId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_States_StateId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Worktracks_Tasks_TaskId",
                table: "Worktracks");

            migrationBuilder.DropIndex(
                name: "IX_Worktracks_TaskId",
                table: "Worktracks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "Worktracks");

            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "Worktasks");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_StateId",
                table: "Worktasks",
                newName: "IX_Worktasks_StateId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_ProjectId",
                table: "Worktasks",
                newName: "IX_Worktasks_ProjectId");

            migrationBuilder.AddColumn<int>(
                name: "WorktaskId",
                table: "Worktracks",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Tokens",
                maxLength: 305,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Worktasks",
                table: "Worktasks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Worktracks_WorktaskId",
                table: "Worktracks",
                column: "WorktaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Worktasks_Projects_ProjectId",
                table: "Worktasks",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Worktasks_States_StateId",
                table: "Worktasks",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Worktracks_Worktasks_WorktaskId",
                table: "Worktracks",
                column: "WorktaskId",
                principalTable: "Worktasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Worktasks_Projects_ProjectId",
                table: "Worktasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Worktasks_States_StateId",
                table: "Worktasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Worktracks_Worktasks_WorktaskId",
                table: "Worktracks");

            migrationBuilder.DropIndex(
                name: "IX_Worktracks_WorktaskId",
                table: "Worktracks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Worktasks",
                table: "Worktasks");

            migrationBuilder.DropColumn(
                name: "WorktaskId",
                table: "Worktracks");

            migrationBuilder.RenameTable(
                name: "Worktasks",
                newName: "Tasks");

            migrationBuilder.RenameIndex(
                name: "IX_Worktasks_StateId",
                table: "Tasks",
                newName: "IX_Tasks_StateId");

            migrationBuilder.RenameIndex(
                name: "IX_Worktasks_ProjectId",
                table: "Tasks",
                newName: "IX_Tasks_ProjectId");

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "Worktracks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Tokens",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 305);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Worktracks_TaskId",
                table: "Worktracks",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Projects_ProjectId",
                table: "Tasks",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_States_StateId",
                table: "Tasks",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Worktracks_Tasks_TaskId",
                table: "Worktracks",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
