using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyago.BusService.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seats_BusId_RowNumber_ColumnNumber",
                table: "Seats");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Seats",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_BusId_Level_RowNumber_ColumnNumber",
                table: "Seats",
                columns: new[] { "BusId", "Level", "RowNumber", "ColumnNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seats_BusId_Level_RowNumber_ColumnNumber",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Seats");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_BusId_RowNumber_ColumnNumber",
                table: "Seats",
                columns: new[] { "BusId", "RowNumber", "ColumnNumber" },
                unique: true);
        }
    }
}
