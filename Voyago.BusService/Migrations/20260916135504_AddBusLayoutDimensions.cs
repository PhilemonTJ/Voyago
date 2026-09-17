using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyago.BusService.Migrations
{
    /// <inheritdoc />
    public partial class AddBusLayoutDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalColumns",
                table: "Buses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRows",
                table: "Buses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalColumns",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "TotalRows",
                table: "Buses");
        }
    }
}
