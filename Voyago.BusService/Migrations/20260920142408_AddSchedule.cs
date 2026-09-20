using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyago.BusService.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginStopId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationStopId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartureTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ArrivalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Buses_BusId",
                        column: x => x.BusId,
                        principalTable: "Buses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schedules_Stops_DestinationStopId",
                        column: x => x.DestinationStopId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schedules_Stops_OriginStopId",
                        column: x => x.OriginStopId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_BusId_DepartureTime",
                table: "Schedules",
                columns: new[] { "BusId", "DepartureTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_DestinationStopId",
                table: "Schedules",
                column: "DestinationStopId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_OriginStopId_DestinationStopId_DepartureTime",
                table: "Schedules",
                columns: new[] { "OriginStopId", "DestinationStopId", "DepartureTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schedules");
        }
    }
}
