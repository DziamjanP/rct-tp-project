using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace course_project.Migrations
{
    /// <inheritdoc />
    public partial class RenameDepartureArrival : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Departure",
                table: "TimeTableEntries",
                newName: "DepartureTime");

            migrationBuilder.RenameColumn(
                name: "Arrival",
                table: "TimeTableEntries",
                newName: "ArrivalTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DepartureTime",
                table: "TimeTableEntries",
                newName: "Departure");

            migrationBuilder.RenameColumn(
                name: "ArrivalTime",
                table: "TimeTableEntries",
                newName: "Arrival");
        }
    }
}
