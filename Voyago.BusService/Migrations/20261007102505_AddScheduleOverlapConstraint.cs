using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyago.BusService.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleOverlapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE EXTENSION IF NOT EXISTS btree_gist;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Schedules"
                ADD CONSTRAINT "CK_Schedules_NoOverlappingActiveSchedules"
                EXCLUDE USING GIST
                (
                    "BusId" WITH =,
                    tstzrange(
                        "DepartureTime",
                        "ArrivalTime",
                        '[)'
                    ) WITH &&
                )
                WHERE ("Status" <> 'Cancelled');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Schedules"
                DROP CONSTRAINT IF EXISTS "CK_Schedules_NoOverlappingActiveSchedules";
                """);
        }
    }
}
