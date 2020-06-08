using Microsoft.EntityFrameworkCore.Migrations;

namespace Timetracker.Entities.Migrations
{
    public partial class RenamingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizedUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rights",
                table: "Rights");

            migrationBuilder.RenameTable(
                name: "Rights",
                newName: "Roles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "LinkedProjects",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RightId = table.Column<byte>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<int>(nullable: false),
                    Accepted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkedProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LinkedProjects_Roles_RightId",
                        column: x => x.RightId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LinkedProjects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LinkedProjects_ProjectId",
                table: "LinkedProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedProjects_RightId",
                table: "LinkedProjects",
                column: "RightId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedProjects_UserId",
                table: "LinkedProjects",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinkedProjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Rights");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rights",
                table: "Rights",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AuthorizedUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Accepted = table.Column<bool>(type: "bit", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    RightId = table.Column<byte>(type: "tinyint", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizedUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizedUsers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorizedUsers_Rights_RightId",
                        column: x => x.RightId,
                        principalTable: "Rights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorizedUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedUsers_ProjectId",
                table: "AuthorizedUsers",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedUsers_RightId",
                table: "AuthorizedUsers",
                column: "RightId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedUsers_UserId",
                table: "AuthorizedUsers",
                column: "UserId");
        }
    }
}
