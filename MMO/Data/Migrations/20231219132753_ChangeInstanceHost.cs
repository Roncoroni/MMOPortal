using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMO.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInstanceHost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConnectionId",
                table: "InstanceHosts",
                newName: "SharedSecret");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SharedSecret",
                table: "InstanceHosts",
                newName: "ConnectionId");
        }
    }
}
