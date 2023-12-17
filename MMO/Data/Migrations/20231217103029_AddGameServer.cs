using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGameServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinInstances",
                table: "GameServerTypes",
                newName: "StartType");

            migrationBuilder.AddColumn<string>(
                name: "MapName",
                table: "GameServerTypes",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "InstanceHosts",
                columns: table => new
                {
                    InstanceHostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConnectionId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanceHosts", x => x.InstanceHostId);
                });

            migrationBuilder.CreateTable(
                name: "GameServers",
                columns: table => new
                {
                    GameServerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameServerTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InstanceHostId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameServers", x => x.GameServerId);
                    table.ForeignKey(
                        name: "FK_GameServers_GameServerTypes_GameServerTypeId",
                        column: x => x.GameServerTypeId,
                        principalTable: "GameServerTypes",
                        principalColumn: "GameServerTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameServers_InstanceHosts_InstanceHostId",
                        column: x => x.InstanceHostId,
                        principalTable: "InstanceHosts",
                        principalColumn: "InstanceHostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameServers_GameServerTypeId",
                table: "GameServers",
                column: "GameServerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GameServers_InstanceHostId",
                table: "GameServers",
                column: "InstanceHostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameServers");

            migrationBuilder.DropTable(
                name: "InstanceHosts");

            migrationBuilder.DropColumn(
                name: "MapName",
                table: "GameServerTypes");

            migrationBuilder.RenameColumn(
                name: "StartType",
                table: "GameServerTypes",
                newName: "MinInstances");
        }
    }
}
