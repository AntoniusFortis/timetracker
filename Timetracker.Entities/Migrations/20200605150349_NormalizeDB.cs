using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timetracker.Entities.Migrations
{
    public partial class NormalizeDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Draft",
                table: "Worktracks");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Rights");

            migrationBuilder.DropColumn(
                name: "IsSigned",
                table: "AuthorizedUsers");

            migrationBuilder.AddColumn<bool>(
                name: "Running",
                table: "Worktracks",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TokenId",
                table: "Users",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tasks",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "States",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Rights",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Projects",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "AuthorizedUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefreshToken = table.Column<string>(maxLength: 50, nullable: false),
                    AccessToken = table.Column<string>(maxLength: 300, nullable: false),
                    TokenExpiredDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropColumn(
                name: "Running",
                table: "Worktracks");

            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Rights");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "AuthorizedUsers");

            migrationBuilder.AddColumn<bool>(
                name: "Draft",
                table: "Worktracks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tasks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "States",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 60);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Rights",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Projects",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "IsSigned",
                table: "AuthorizedUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
