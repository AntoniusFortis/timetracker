using Microsoft.EntityFrameworkCore.Migrations;

namespace Timetracker.Entities.Migrations
{
    public partial class RenameRightIdField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinkedProjects_Roles_RightId",
                table: "LinkedProjects");

            migrationBuilder.DropIndex(
                name: "IX_LinkedProjects_RightId",
                table: "LinkedProjects");

            migrationBuilder.DropColumn(
                name: "RightId",
                table: "LinkedProjects");

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Tokens",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(305)",
                oldMaxLength: 305);

            migrationBuilder.AddColumn<byte>(
                name: "RoleId",
                table: "LinkedProjects",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_LinkedProjects_RoleId",
                table: "LinkedProjects",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkedProjects_Roles_RoleId",
                table: "LinkedProjects",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinkedProjects_Roles_RoleId",
                table: "LinkedProjects");

            migrationBuilder.DropIndex(
                name: "IX_LinkedProjects_RoleId",
                table: "LinkedProjects");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "LinkedProjects");

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Tokens",
                type: "nvarchar(305)",
                maxLength: 305,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 1024);

            migrationBuilder.AddColumn<byte>(
                name: "RightId",
                table: "LinkedProjects",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_LinkedProjects_RightId",
                table: "LinkedProjects",
                column: "RightId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkedProjects_Roles_RightId",
                table: "LinkedProjects",
                column: "RightId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
