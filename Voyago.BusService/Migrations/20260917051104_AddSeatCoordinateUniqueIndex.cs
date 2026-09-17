using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyago.BusService.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatCoordinateUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Seats_BusId_RowNumber_ColumnNumber",
                table: "Seats",
                columns: new[] { "BusId", "RowNumber", "ColumnNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seats_BusId_RowNumber_ColumnNumber",
                table: "Seats");
        }
    }
}
