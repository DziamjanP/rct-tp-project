using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace course_project.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerkGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    FixedPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    Discount = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerkGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricePolicies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PricePerKm = table.Column<double>(type: "double precision", nullable: true),
                    PricePerStation = table.Column<double>(type: "double precision", nullable: true),
                    FixedPrice = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CityName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Password = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Salt = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Phone = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    Passport = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Surname = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricePolicyPerkGroups",
                columns: table => new
                {
                    PricePolicyId = table.Column<long>(type: "bigint", nullable: false),
                    PerkGroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricePolicyPerkGroups", x => new { x.PricePolicyId, x.PerkGroupId });
                    table.ForeignKey(
                        name: "FK_PricePolicyPerkGroups_PerkGroups_PerkGroupId",
                        column: x => x.PerkGroupId,
                        principalTable: "PerkGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PricePolicyPerkGroups_PricePolicies_PricePolicyId",
                        column: x => x.PricePolicyId,
                        principalTable: "PricePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trains",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationId = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trains_Stations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trains_Stations_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trains_TrainTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "TrainTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeTableEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainId = table.Column<long>(type: "bigint", nullable: false),
                    Departure = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Arrival = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StopInfo = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    PricePolicyId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTableEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeTableEntries_PricePolicies_PricePolicyId",
                        column: x => x.PricePolicyId,
                        principalTable: "PricePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TimeTableEntries_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketLocks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntryId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Paid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Sum = table.Column<decimal>(type: "numeric", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntryId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_TimeTableEntries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "TimeTableEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LockId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Successful = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sum = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_TicketLocks_LockId",
                        column: x => x.LockId,
                        principalTable: "TicketLocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_LockId",
                table: "Payments",
                column: "LockId");

            migrationBuilder.CreateIndex(
                name: "IX_PricePolicyPerkGroups_PerkGroupId",
                table: "PricePolicyPerkGroups",
                column: "PerkGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLocks_EntryId",
                table: "TicketLocks",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLocks_UserId",
                table: "TicketLocks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EntryId",
                table: "Tickets",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTableEntries_PricePolicyId",
                table: "TimeTableEntries",
                column: "PricePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTableEntries_TrainId",
                table: "TimeTableEntries",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_DestinationId",
                table: "Trains",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_SourceId",
                table: "Trains",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_TypeId",
                table: "Trains",
                column: "TypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PricePolicyPerkGroups");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "TicketLocks");

            migrationBuilder.DropTable(
                name: "PerkGroups");

            migrationBuilder.DropTable(
                name: "TimeTableEntries");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PricePolicies");

            migrationBuilder.DropTable(
                name: "Trains");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "TrainTypes");
        }
    }
}
