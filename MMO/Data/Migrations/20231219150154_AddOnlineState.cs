using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOnlineState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastHeartbeat",
                table: "InstanceHosts",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Online",
                table: "InstanceHosts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastHeartbeat",
                table: "InstanceHosts");

            migrationBuilder.DropColumn(
                name: "Online",
                table: "InstanceHosts");
        }
    }
}
