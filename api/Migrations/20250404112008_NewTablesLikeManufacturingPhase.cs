using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTexPact.Migrations
{
    /// <inheritdoc />
    public partial class NewTablesLikeManufacturingPhase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManufacturingPhases_PlantFloorSectionId",
                table: "ManufacturingPhases");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingPhases_PlantFloorSectionId",
                table: "ManufacturingPhases",
                column: "PlantFloorSectionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManufacturingPhases_PlantFloorSectionId",
                table: "ManufacturingPhases");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingPhases_PlantFloorSectionId",
                table: "ManufacturingPhases",
                column: "PlantFloorSectionId");
        }
    }
}
