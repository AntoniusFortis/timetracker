using Microsoft.EntityFrameworkCore.Migrations;

namespace Timetracker.Entities.Migrations
{
    public partial class AddPassTable_Part2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Pass_PassId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pass",
                table: "Pass");

            migrationBuilder.RenameTable(
                name: "Pass",
                newName: "Passes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Passes",
                table: "Passes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Passes_PassId",
                table: "Users",
                column: "PassId",
                principalTable: "Passes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql( @"
                CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Login_PassId_Inc] ON [dbo].[Users]
                    (
                        [Login] ASC,
                        [PassId] ASC
                    )
                    INCLUDE([TokenId]);

                CREATE UNIQUE NONCLUSTERED INDEX [IX_LinkedProjects_UserId_ProjectId_Inc] ON [dbo].[LinkedProjects]
                (
	                [UserId] ASC,
	                [ProjectId] ASC
                )
                INCLUDE ( 	[Accepted],
	                [RoleId]);

                CREATE NONCLUSTERED INDEX [IX_Worktracks_UserId_WorktaskId_Running] ON [dbo].[Worktracks]
                (
	                [UserId] ASC,
	                [WorktaskId] ASC,
	                [Running] ASC
                );

                CREATE NONCLUSTERED INDEX [IX_Worktracks_UserId_WorktaskId] ON [dbo].[Worktracks]
                (
	                [UserId] ASC,
	                [WorktaskId] ASC
                );

                CREATE NONCLUSTERED INDEX [IX_Worktracks_UserId_Running] ON [dbo].[Worktracks]
                (
	                [UserId] ASC,
	                [Running] ASC
                );

                CREATE NONCLUSTERED INDEX [IX_Worktracks_UserId_TaskId_Inc] ON [dbo].[Worktracks]
                (
	                [UserId] ASC,
	                [WorktaskId] ASC
                )
                INCLUDE ([StartedTime], [StoppedTime]);
            " );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Passes_PassId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Passes",
                table: "Passes");

            migrationBuilder.RenameTable(
                name: "Passes",
                newName: "Pass");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pass",
                table: "Pass",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Pass_PassId",
                table: "Users",
                column: "PassId",
                principalTable: "Pass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
