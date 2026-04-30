using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTexPact.Migrations
{
    /// <inheritdoc />
    public partial class NewTablesLikeManufacturingOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManufacturingOrderId",
                table: "ItemOfRawMaterial",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemOfRawMaterial_ManufacturingOrderId",
                table: "ItemOfRawMaterial",
                column: "ManufacturingOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemOfRawMaterial_ManufacturingOrders_ManufacturingOrderId",
                table: "ItemOfRawMaterial",
                column: "ManufacturingOrderId",
                principalTable: "ManufacturingOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemOfRawMaterial_ManufacturingOrders_ManufacturingOrderId",
                table: "ItemOfRawMaterial");

            migrationBuilder.DropIndex(
                name: "IX_ItemOfRawMaterial_ManufacturingOrderId",
                table: "ItemOfRawMaterial");

            migrationBuilder.DropColumn(
                name: "ManufacturingOrderId",
                table: "ItemOfRawMaterial");
        }
    }
}
