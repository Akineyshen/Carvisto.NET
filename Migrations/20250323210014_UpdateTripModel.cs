using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carvisto.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTripModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "VehicleModel",
                table: "Trips",
                newName: "Comments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "Trips",
                newName: "VehicleModel");

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
