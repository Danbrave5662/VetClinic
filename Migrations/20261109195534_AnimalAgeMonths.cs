using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetClinic.Migrations
{
    /// <inheritdoc />
    public partial class AnimalAgeMonths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgeMonths",
                table: "Animals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeMonths",
                table: "Animals");
        }
    }
}
