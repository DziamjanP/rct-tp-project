using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace course_project.Migrations
{
    /// <inheritdoc />
    public partial class RemovePricePerStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerStation",
                table: "PricePolicies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PricePerStation",
                table: "PricePolicies",
                type: "double precision",
                nullable: true);
        }
    }
}
