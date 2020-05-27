using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Timetracker.Entities.Migrations
{
    public partial class MissingValuesFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql( @"INSERT INTO [dbo].[States] ([Title]) VALUES ('Новый'), ('Активный'), ('Завершающая фаза'), ('Закончен')" );
            migrationBuilder.Sql( @"INSERT INTO [dbo].[Rights] ([Name]) VALUES ('Администратор'), ('Пользователь')" );

            migrationBuilder.AlterColumn<string>(
                name: "BirthDate",
                table: "Users",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedUsers_RightId",
                table: "AuthorizedUsers",
                column: "RightId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorizedUsers_Rights_RightId",
                table: "AuthorizedUsers",
                column: "RightId",
                principalTable: "Rights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorizedUsers_Rights_RightId",
                table: "AuthorizedUsers");

            migrationBuilder.DropIndex(
                name: "IX_AuthorizedUsers_RightId",
                table: "AuthorizedUsers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 30,
                oldNullable: true);
        }
    }
}
