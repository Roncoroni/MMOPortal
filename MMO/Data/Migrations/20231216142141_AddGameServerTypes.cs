using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGameServerTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameServerTypes",
                columns: table => new
                {
                    GameServerTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MinInstances = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameServerTypes", x => x.GameServerTypeId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameServerTypes");
        }
    }
}
