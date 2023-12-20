using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "InstanceHosts",
                type: "TEXT",
                maxLength: 45,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "InstanceHosts");
        }
    }
}
