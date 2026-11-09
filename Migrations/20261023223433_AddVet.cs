using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetClinic.Migrations
{
    /// <inheritdoc />
    public partial class AddVet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VetId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Vets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_VetId",
                table: "Appointments",
                column: "VetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Vets_VetId",
                table: "Appointments",
                column: "VetId",
                principalTable: "Vets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Vets_VetId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Vets");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_VetId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "VetId",
                table: "Appointments");
        }
    }
}
