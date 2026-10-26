using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyago.BusService.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatHoldExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "HeldUntil",
                table: "ScheduleSeats",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeldUntil",
                table: "ScheduleSeats");
        }
    }
}
