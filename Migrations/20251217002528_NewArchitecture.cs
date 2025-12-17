using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace course_project.Migrations
{
    /// <inheritdoc />
    public partial class NewArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_TicketLocks_LockId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "TicketLocks");

            migrationBuilder.RenameColumn(
                name: "LockId",
                table: "Payments",
                newName: "BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_LockId",
                table: "Payments",
                newName: "IX_Payments_BookingId");

            migrationBuilder.CreateTable(
                name: "TicketBookings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntryId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Paid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Sum = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketBookings_TimeTableEntries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "TimeTableEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketBookings_EntryId",
                table: "TicketBookings",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketBookings_UserId",
                table: "TicketBookings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_TicketBookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "TicketBookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_TicketBookings_BookingId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "TicketBookings");

            migrationBuilder.RenameColumn(
                name: "BookingId",
                table: "Payments",
                newName: "LockId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                newName: "IX_Payments_LockId");

            migrationBuilder.CreateTable(
                name: "TicketLocks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntryId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InvoiceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Paid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Sum = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketLocks_TimeTableEntries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "TimeTableEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketLocks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketLocks_EntryId",
                table: "TicketLocks",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLocks_UserId",
                table: "TicketLocks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_TicketLocks_LockId",
                table: "Payments",
                column: "LockId",
                principalTable: "TicketLocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
