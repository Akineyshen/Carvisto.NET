using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carvisto.Migrations
{
    /// <inheritdoc />
    public partial class AddAplicationUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "AspNetUsers");
        }
    }
}
