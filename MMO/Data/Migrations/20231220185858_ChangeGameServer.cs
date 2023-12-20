using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMO.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGameServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GameServers",
                table: "GameServers");

            migrationBuilder.DropIndex(
                name: "IX_GameServers_InstanceHostId",
                table: "GameServers");

            migrationBuilder.DropColumn(
                name: "GameServerId",
                table: "GameServers");

            migrationBuilder.AddColumn<ushort>(
                name: "Port",
                table: "GameServers",
                type: "INTEGER",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameServers",
                table: "GameServers",
                columns: new[] { "InstanceHostId", "Port" });

            migrationBuilder.CreateIndex(
                name: "IX_GameServerTypes_Name",
                table: "GameServerTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameServerTypes_Name",
                table: "GameServerTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameServers",
                table: "GameServers");

            migrationBuilder.DropColumn(
                name: "Port",
                table: "GameServers");

            migrationBuilder.AddColumn<Guid>(
                name: "GameServerId",
                table: "GameServers",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameServers",
                table: "GameServers",
                column: "GameServerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameServers_InstanceHostId",
                table: "GameServers",
                column: "InstanceHostId");
        }
    }
}
