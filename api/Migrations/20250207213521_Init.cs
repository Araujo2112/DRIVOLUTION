using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApiTexPact.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Containers",
                columns: table => new
                {
                    ContainerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContainerCode = table.Column<string>(type: "text", nullable: false),
                    ContainerName = table.Column<string>(type: "text", nullable: false),
                    ContainerVolume = table.Column<float>(type: "real", nullable: false),
                    Activate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Containers", x => x.ContainerId);
                });

            migrationBuilder.CreateTable(
                name: "PlantFloorSection",
                columns: table => new
                {
                    SectionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SectionCode = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantFloorSection", x => x.SectionId);
                });

            migrationBuilder.CreateTable(
                name: "RawMaterial",
                columns: table => new
                {
                    RawId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Info = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMaterial", x => x.RawId);
                });

            migrationBuilder.CreateTable(
                name: "ItemInContainer",
                columns: table => new
                {
                    ItemInContainerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "text", nullable: false),
                    ContainerId = table.Column<int>(type: "integer", nullable: false),
                    DateTimeIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateTimeOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemInContainer", x => x.ItemInContainerId);
                    table.ForeignKey(
                        name: "FK_ItemInContainer_Containers_ContainerId",
                        column: x => x.ContainerId,
                        principalTable: "Containers",
                        principalColumn: "ContainerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Checkpoints",
                columns: table => new
                {
                    CheckpointId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CheckpointCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    SectionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkpoints", x => x.CheckpointId);
                    table.ForeignKey(
                        name: "FK_Checkpoints_PlantFloorSection_SectionId",
                        column: x => x.SectionId,
                        principalTable: "PlantFloorSection",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContainerLocalization",
                columns: table => new
                {
                    ContainerId = table.Column<int>(type: "integer", nullable: false),
                    SectionId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerLocalization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerLocalization_Containers_ContainerId",
                        column: x => x.ContainerId,
                        principalTable: "Containers",
                        principalColumn: "ContainerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContainerLocalization_PlantFloorSection_SectionId",
                        column: x => x.SectionId,
                        principalTable: "PlantFloorSection",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingPhaseModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Info = table.Column<string>(type: "text", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    PlantFloorSectionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingPhaseModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturingPhaseModel_PlantFloorSection_PlantFloorSection~",
                        column: x => x.PlantFloorSectionId,
                        principalTable: "PlantFloorSection",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LotOfRawMaterial",
                columns: table => new
                {
                    LotId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotCode = table.Column<string>(type: "text", nullable: false),
                    LotNumber = table.Column<string>(type: "text", nullable: false),
                    LotQuantity = table.Column<int>(type: "integer", nullable: false),
                    LotUnit = table.Column<string>(type: "text", nullable: false),
                    RawMaterialId = table.Column<int>(type: "integer", nullable: false),
                    HistoricalWeights = table.Column<List<int>>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotOfRawMaterial", x => x.LotId);
                    table.ForeignKey(
                        name: "FK_LotOfRawMaterial_RawMaterial_RawMaterialId",
                        column: x => x.RawMaterialId,
                        principalTable: "RawMaterial",
                        principalColumn: "RawId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    WatchId = table.Column<string>(type: "text", nullable: true),
                    ManufacturingPhaseId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_ManufacturingPhaseModel_ManufacturingPhaseId",
                        column: x => x.ManufacturingPhaseId,
                        principalTable: "ManufacturingPhaseModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItemOfRawMaterial",
                columns: table => new
                {
                    ItemRawId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    LotOfRawMaterialId = table.Column<int>(type: "integer", nullable: false),
                    ItemInContainerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemOfRawMaterial", x => x.ItemRawId);
                    table.ForeignKey(
                        name: "FK_ItemOfRawMaterial_ItemInContainer_ItemInContainerId",
                        column: x => x.ItemInContainerId,
                        principalTable: "ItemInContainer",
                        principalColumn: "ItemInContainerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemOfRawMaterial_LotOfRawMaterial_LotOfRawMaterialId",
                        column: x => x.LotOfRawMaterialId,
                        principalTable: "LotOfRawMaterial",
                        principalColumn: "LotId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SectionAdminModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    PlantFloorSectionId = table.Column<int>(type: "integer", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionAdminModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionAdminModel_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectionAdminModel_PlantFloorSection_PlantFloorSectionId",
                        column: x => x.PlantFloorSectionId,
                        principalTable: "PlantFloorSection",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemLocalization",
                columns: table => new
                {
                    ItemLocalizationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemRawId = table.Column<int>(type: "integer", nullable: false),
                    ContainerLocalizationId = table.Column<int>(type: "integer", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemLocalization", x => x.ItemLocalizationId);
                    table.ForeignKey(
                        name: "FK_ItemLocalization_ContainerLocalization_ContainerLocalizatio~",
                        column: x => x.ContainerLocalizationId,
                        principalTable: "ContainerLocalization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemLocalization_ItemOfRawMaterial_ItemRawId",
                        column: x => x.ItemRawId,
                        principalTable: "ItemOfRawMaterial",
                        principalColumn: "ItemRawId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "FirstName", "LastName", "ManufacturingPhaseId", "Password", "Username", "WatchId" },
                values: new object[] { 1, "John", "Doe", null, "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", "admin", null });

            migrationBuilder.CreateIndex(
                name: "IX_Checkpoints_SectionId",
                table: "Checkpoints",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerLocalization_ContainerId",
                table: "ContainerLocalization",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerLocalization_SectionId",
                table: "ContainerLocalization",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManufacturingPhaseId",
                table: "Employees",
                column: "ManufacturingPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInContainer_ContainerId",
                table: "ItemInContainer",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLocalization_ContainerLocalizationId",
                table: "ItemLocalization",
                column: "ContainerLocalizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLocalization_ItemRawId",
                table: "ItemLocalization",
                column: "ItemRawId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemOfRawMaterial_ItemInContainerId",
                table: "ItemOfRawMaterial",
                column: "ItemInContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemOfRawMaterial_LotOfRawMaterialId",
                table: "ItemOfRawMaterial",
                column: "LotOfRawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_LotOfRawMaterial_RawMaterialId",
                table: "LotOfRawMaterial",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingPhaseModel_PlantFloorSectionId",
                table: "ManufacturingPhaseModel",
                column: "PlantFloorSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionAdminModel_EmployeeId",
                table: "SectionAdminModel",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionAdminModel_PlantFloorSectionId",
                table: "SectionAdminModel",
                column: "PlantFloorSectionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Checkpoints");

            migrationBuilder.DropTable(
                name: "ItemLocalization");

            migrationBuilder.DropTable(
                name: "SectionAdminModel");

            migrationBuilder.DropTable(
                name: "ContainerLocalization");

            migrationBuilder.DropTable(
                name: "ItemOfRawMaterial");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "ItemInContainer");

            migrationBuilder.DropTable(
                name: "LotOfRawMaterial");

            migrationBuilder.DropTable(
                name: "ManufacturingPhaseModel");

            migrationBuilder.DropTable(
                name: "Containers");

            migrationBuilder.DropTable(
                name: "RawMaterial");

            migrationBuilder.DropTable(
                name: "PlantFloorSection");
        }
    }
}
