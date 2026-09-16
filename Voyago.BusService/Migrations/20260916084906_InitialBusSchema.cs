using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyago.BusService.Migrations
{
    /// <inheritdoc />
    public partial class InitialBusSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BusNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BusName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BusType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buses_OperatorId",
                table: "Buses",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Buses_OperatorId_BusNumber",
                table: "Buses",
                columns: new[] { "OperatorId", "BusNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buses_RegistrationNumber",
                table: "Buses",
                column: "RegistrationNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buses");
        }
    }
}
