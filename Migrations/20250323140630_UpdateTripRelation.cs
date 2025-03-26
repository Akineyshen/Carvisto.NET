using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carvisto.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTripRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DepartureTime",
                table: "Trips",
                newName: "DepartureDateTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DepartureDateTime",
                table: "Trips",
                newName: "DepartureTime");
        }
    }
}
