using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTexPact.Migrations
{
    /// <inheritdoc />
    public partial class MigrationItemLocalizationWithBackFkToItemOfRawMatrial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemOfRawMaterialModelItemRawId",
                table: "ItemLocalization",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemLocalization_ItemOfRawMaterialModelItemRawId",
                table: "ItemLocalization",
                column: "ItemOfRawMaterialModelItemRawId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemLocalization_ItemOfRawMaterial_ItemOfRawMaterialModelIt~",
                table: "ItemLocalization",
                column: "ItemOfRawMaterialModelItemRawId",
                principalTable: "ItemOfRawMaterial",
                principalColumn: "ItemRawId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemLocalization_ItemOfRawMaterial_ItemOfRawMaterialModelIt~",
                table: "ItemLocalization");

            migrationBuilder.DropIndex(
                name: "IX_ItemLocalization_ItemOfRawMaterialModelItemRawId",
                table: "ItemLocalization");

            migrationBuilder.DropColumn(
                name: "ItemOfRawMaterialModelItemRawId",
                table: "ItemLocalization");
        }
    }
}
