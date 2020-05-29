using Microsoft.EntityFrameworkCore.Migrations;

namespace Timetracker.Entities.Migrations
{
    public partial class FixRussianCoding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql( @"
				UPDATE [dbo].[Rights]
				SET [Name] = N'Администратор'
				WHERE [Id] = 1

				UPDATE [dbo].[Rights]
				SET [Name] = N'Пользователь'
				WHERE [Id] = 2
            " );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql( @"
				UPDATE [dbo].[Rights]
				SET [Name] = 'Администратор'
				WHERE [Id] = 1

				UPDATE [dbo].[Rights]
				SET [Name] = 'Пользователь'
				WHERE [Id] = 2
            " );
        }
    }
}
