using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace course_project.Migrations
{
    /// <inheritdoc />
    public partial class EnsureCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeTableEntries_PricePolicies_PricePolicyId",
                table: "TimeTableEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Trains_Stations_DestinationId",
                table: "Trains");

            migrationBuilder.DropForeignKey(
                name: "FK_Trains_Stations_SourceId",
                table: "Trains");

            migrationBuilder.DropForeignKey(
                name: "FK_Trains_TrainTypes_TypeId",
                table: "Trains");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTableEntries_PricePolicies_PricePolicyId",
                table: "TimeTableEntries",
                column: "PricePolicyId",
                principalTable: "PricePolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trains_Stations_DestinationId",
                table: "Trains",
                column: "DestinationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trains_Stations_SourceId",
                table: "Trains",
                column: "SourceId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trains_TrainTypes_TypeId",
                table: "Trains",
                column: "TypeId",
                principalTable: "TrainTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeTableEntries_PricePolicies_PricePolicyId",
                table: "TimeTableEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Trains_Stations_DestinationId",
                table: "Trains");

            migrationBuilder.DropForeignKey(
                name: "FK_Trains_Stations_SourceId",
                table: "Trains");

            migrationBuilder.DropForeignKey(
                name: "FK_Trains_TrainTypes_TypeId",
                table: "Trains");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTableEntries_PricePolicies_PricePolicyId",
                table: "TimeTableEntries",
                column: "PricePolicyId",
                principalTable: "PricePolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Trains_Stations_DestinationId",
                table: "Trains",
                column: "DestinationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trains_Stations_SourceId",
                table: "Trains",
                column: "SourceId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trains_TrainTypes_TypeId",
                table: "Trains",
                column: "TypeId",
                principalTable: "TrainTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
