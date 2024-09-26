using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L4d2ServerQuery.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithTagAndFavoriteServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreateAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreateAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Host = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    Desc = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Field1 = table.Column<string>(type: "TEXT", nullable: true),
                    Field2 = table.Column<string>(type: "TEXT", nullable: true),
                    TagId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteServers_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteServers_TagId",
                table: "FavoriteServers",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteServers");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
