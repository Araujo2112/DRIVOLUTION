using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApiTexPact.Migrations
{
    /// <inheritdoc />
    public partial class NewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_ManufacturingPhaseModel_ManufacturingPhaseId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemOfRawMaterial_LotOfRawMaterial_LotOfRawMaterialId",
                table: "ItemOfRawMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_LotOfRawMaterial_RawMaterial_RawMaterialId",
                table: "LotOfRawMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_ManufacturingPhaseModel_PlantFloorSection_PlantFloorSection~",
                table: "ManufacturingPhaseModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManufacturingPhaseModel",
                table: "ManufacturingPhaseModel");

            migrationBuilder.RenameTable(
                name: "ManufacturingPhaseModel",
                newName: "ManufacturingPhases");

            migrationBuilder.RenameColumn(
                name: "Info",
                table: "ManufacturingPhases",
                newName: "PhaseInfo");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "ManufacturingPhases",
                newName: "PhaseDuration");

            migrationBuilder.RenameIndex(
                name: "IX_ManufacturingPhaseModel_PlantFloorSectionId",
                table: "ManufacturingPhases",
                newName: "IX_ManufacturingPhases_PlantFloorSectionId");

            migrationBuilder.AddColumn<int>(
                name: "ManufacturingOrderPhaseId",
                table: "ItemOfRawMaterial",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturingPhaseId",
                table: "ManufacturingPhases",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManufacturingPhases",
                table: "ManufacturingPhases",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FiscalNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Info = table.Column<string>(type: "text", nullable: false),
                    ProductId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProcessName = table.Column<string>(type: "text", nullable: false),
                    Info = table.Column<string>(type: "text", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturingProcesses_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductLots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotNumber = table.Column<string>(type: "text", nullable: false),
                    LotUnit = table.Column<string>(type: "text", nullable: false),
                    LotQuantity = table.Column<int>(type: "integer", nullable: false),
                    Ready = table.Column<bool>(type: "boolean", nullable: false),
                    ProductLotId = table.Column<string>(type: "text", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductLots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductLots_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingProcessPhases",
                columns: table => new
                {
                    ManufacturingPhaseId = table.Column<int>(type: "integer", nullable: false),
                    ManufacturingProcessId = table.Column<int>(type: "integer", nullable: false),
                    NumberStepOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingProcessPhases", x => new { x.ManufacturingPhaseId, x.ManufacturingProcessId });
                    table.ForeignKey(
                        name: "FK_ManufacturingProcessPhases_ManufacturingPhases_Manufacturin~",
                        column: x => x.ManufacturingPhaseId,
                        principalTable: "ManufacturingPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManufacturingProcessPhases_ManufacturingProcesses_Manufactu~",
                        column: x => x.ManufacturingProcessId,
                        principalTable: "ManufacturingProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PhaseInfo = table.Column<string>(type: "text", nullable: false),
                    PhaseDuration = table.Column<int>(type: "integer", nullable: false),
                    ManufacturingOrderId = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ManufacturingProcessId = table.Column<int>(type: "integer", nullable: false),
                    ProductLotId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturingOrders_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManufacturingOrders_ManufacturingProcesses_ManufacturingPro~",
                        column: x => x.ManufacturingProcessId,
                        principalTable: "ManufacturingProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManufacturingOrders_ProductLots_ProductLotId",
                        column: x => x.ProductLotId,
                        principalTable: "ProductLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingOrderHistories",
                columns: table => new
                {
                    ManufacturingOrderId = table.Column<int>(type: "integer", nullable: false),
                    PlantFloorSectionId = table.Column<int>(type: "integer", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StatusName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingOrderHistories", x => new { x.ManufacturingOrderId, x.PlantFloorSectionId });
                    table.ForeignKey(
                        name: "FK_ManufacturingOrderHistories_ManufacturingOrders_Manufacturi~",
                        column: x => x.ManufacturingOrderId,
                        principalTable: "ManufacturingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManufacturingOrderHistories_PlantFloorSection_PlantFloorSec~",
                        column: x => x.PlantFloorSectionId,
                        principalTable: "PlantFloorSection",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingOrderPhases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomizationParams = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    SheduleInit = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateTimeInit = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateTimeEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ManufacturingOrderId = table.Column<int>(type: "integer", nullable: false),
                    ManufacturingPhaseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingOrderPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturingOrderPhases_ManufacturingOrders_ManufacturingO~",
                        column: x => x.ManufacturingOrderId,
                        principalTable: "ManufacturingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManufacturingOrderPhases_ManufacturingPhases_ManufacturingP~",
                        column: x => x.ManufacturingPhaseId,
                        principalTable: "ManufacturingPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemOfRawMaterial_ManufacturingOrderPhaseId",
                table: "ItemOfRawMaterial",
                column: "ManufacturingOrderPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOrderHistories_PlantFloorSectionId",
                table: "ManufacturingOrderHistories",
                column: "PlantFloorSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOrderPhases_ManufacturingOrderId",
                table: "ManufacturingOrderPhases",
                column: "ManufacturingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOrderPhases_ManufacturingPhaseId",
                table: "ManufacturingOrderPhases",
                column: "ManufacturingPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOrders_ClientId",
                table: "ManufacturingOrders",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOrders_ManufacturingProcessId",
                table: "ManufacturingOrders",
                column: "ManufacturingProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOrders_ProductLotId",
                table: "ManufacturingOrders",
                column: "ProductLotId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingProcesses_ProductId",
                table: "ManufacturingProcesses",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingProcessPhases_ManufacturingProcessId",
                table: "ManufacturingProcessPhases",
                column: "ManufacturingProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLots_ProductId",
                table: "ProductLots",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_ManufacturingPhases_ManufacturingPhaseId",
                table: "Employees",
                column: "ManufacturingPhaseId",
                principalTable: "ManufacturingPhases",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemOfRawMaterial_LotOfRawMaterial_LotOfRawMaterialId",
                table: "ItemOfRawMaterial",
                column: "LotOfRawMaterialId",
                principalTable: "LotOfRawMaterial",
                principalColumn: "LotId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemOfRawMaterial_ManufacturingOrderPhases_ManufacturingOrd~",
                table: "ItemOfRawMaterial",
                column: "ManufacturingOrderPhaseId",
                principalTable: "ManufacturingOrderPhases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LotOfRawMaterial_RawMaterial_RawMaterialId",
                table: "LotOfRawMaterial",
                column: "RawMaterialId",
                principalTable: "RawMaterial",
                principalColumn: "RawId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ManufacturingPhases_PlantFloorSection_PlantFloorSectionId",
                table: "ManufacturingPhases",
                column: "PlantFloorSectionId",
                principalTable: "PlantFloorSection",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_ManufacturingPhases_ManufacturingPhaseId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemOfRawMaterial_LotOfRawMaterial_LotOfRawMaterialId",
                table: "ItemOfRawMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemOfRawMaterial_ManufacturingOrderPhases_ManufacturingOrd~",
                table: "ItemOfRawMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_LotOfRawMaterial_RawMaterial_RawMaterialId",
                table: "LotOfRawMaterial");

            migrationBuilder.DropForeignKey(
                name: "FK_ManufacturingPhases_PlantFloorSection_PlantFloorSectionId",
                table: "ManufacturingPhases");

            migrationBuilder.DropTable(
                name: "ManufacturingOrderHistories");

            migrationBuilder.DropTable(
                name: "ManufacturingOrderPhases");

            migrationBuilder.DropTable(
                name: "ManufacturingProcessPhases");

            migrationBuilder.DropTable(
                name: "ManufacturingOrders");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "ManufacturingProcesses");

            migrationBuilder.DropTable(
                name: "ProductLots");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ItemOfRawMaterial_ManufacturingOrderPhaseId",
                table: "ItemOfRawMaterial");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManufacturingPhases",
                table: "ManufacturingPhases");

            migrationBuilder.DropColumn(
                name: "ManufacturingOrderPhaseId",
                table: "ItemOfRawMaterial");

            migrationBuilder.DropColumn(
                name: "ManufacturingPhaseId",
                table: "ManufacturingPhases");

            migrationBuilder.RenameTable(
                name: "ManufacturingPhases",
                newName: "ManufacturingPhaseModel");

            migrationBuilder.RenameColumn(
                name: "PhaseInfo",
                table: "ManufacturingPhaseModel",
                newName: "Info");

            migrationBuilder.RenameColumn(
                name: "PhaseDuration",
                table: "ManufacturingPhaseModel",
                newName: "Duration");

            migrationBuilder.RenameIndex(
                name: "IX_ManufacturingPhases_PlantFloorSectionId",
                table: "ManufacturingPhaseModel",
                newName: "IX_ManufacturingPhaseModel_PlantFloorSectionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManufacturingPhaseModel",
                table: "ManufacturingPhaseModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_ManufacturingPhaseModel_ManufacturingPhaseId",
                table: "Employees",
                column: "ManufacturingPhaseId",
                principalTable: "ManufacturingPhaseModel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemOfRawMaterial_LotOfRawMaterial_LotOfRawMaterialId",
                table: "ItemOfRawMaterial",
                column: "LotOfRawMaterialId",
                principalTable: "LotOfRawMaterial",
                principalColumn: "LotId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LotOfRawMaterial_RawMaterial_RawMaterialId",
                table: "LotOfRawMaterial",
                column: "RawMaterialId",
                principalTable: "RawMaterial",
                principalColumn: "RawId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManufacturingPhaseModel_PlantFloorSection_PlantFloorSection~",
                table: "ManufacturingPhaseModel",
                column: "PlantFloorSectionId",
                principalTable: "PlantFloorSection",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
