using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Drivolution.Migrations
{
    /// <inheritdoc />
    public partial class InitDrivolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_order",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<string>(type: "text", nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    customer_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_order", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manufacturing_phase",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    estimated_duration = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manufacturing_phase", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "material",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item = table.Column<string>(type: "text", nullable: false),
                    part_number = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "model",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "production_line",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    capacity = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_line", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resource",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_human = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manufacturing_order",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    client_order_id = table.Column<int>(type: "integer", nullable: false),
                    manufacturing_order_number = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manufacturing_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_manufacturing_order_client_order_client_order_id",
                        column: x => x.client_order_id,
                        principalTable: "client_order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "config",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_id = table.Column<int>(type: "integer", nullable: false),
                    item = table.Column<string>(type: "text", nullable: false),
                    default_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_config", x => x.id);
                    table.ForeignKey(
                        name: "FK_config_model_model_id",
                        column: x => x.model_id,
                        principalTable: "model",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "model_material",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_id = table.Column<int>(type: "integer", nullable: false),
                    material_id = table.Column<int>(type: "integer", nullable: false),
                    manufacturing_phase_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    unit = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_material", x => x.id);
                    table.ForeignKey(
                        name: "FK_model_material_manufacturing_phase_manufacturing_phase_id",
                        column: x => x.manufacturing_phase_id,
                        principalTable: "manufacturing_phase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_model_material_material_material_id",
                        column: x => x.material_id,
                        principalTable: "material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_model_material_model_model_id",
                        column: x => x.model_id,
                        principalTable: "model",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "phase_sequence",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order = table.Column<int>(type: "integer", nullable: false),
                    manufacturing_phase_id = table.Column<int>(type: "integer", nullable: false),
                    model_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phase_sequence", x => x.id);
                    table.ForeignKey(
                        name: "FK_phase_sequence_manufacturing_phase_manufacturing_phase_id",
                        column: x => x.manufacturing_phase_id,
                        principalTable: "manufacturing_phase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_phase_sequence_model_model_id",
                        column: x => x.model_id,
                        principalTable: "model",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "support",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    production_line_id = table.Column<int>(type: "integer", nullable: false),
                    rfid_tag = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support", x => x.id);
                    table.ForeignKey(
                        name: "FK_support_production_line_production_line_id",
                        column: x => x.production_line_id,
                        principalTable: "production_line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workstation",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    production_line_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workstation", x => x.id);
                    table.ForeignKey(
                        name: "FK_workstation_production_line_production_line_id",
                        column: x => x.production_line_id,
                        principalTable: "production_line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    manufacturing_order_id = table.Column<int>(type: "integer", nullable: false),
                    model_id = table.Column<int>(type: "integer", nullable: false),
                    serial_number = table.Column<string>(type: "text", nullable: true),
                    lot_number = table.Column<string>(type: "text", nullable: true),
                    color_code = table.Column<string>(type: "text", nullable: true),
                    production_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_manufacturing_order_manufacturing_order_id",
                        column: x => x.manufacturing_order_id,
                        principalTable: "manufacturing_order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_model_model_id",
                        column: x => x.model_id,
                        principalTable: "model",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "localization_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    support_id = table.Column<int>(type: "integer", nullable: false),
                    workstation_id = table.Column<int>(type: "integer", nullable: false),
                    datetime_ini = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    datetime_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_localization_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_localization_history_support_support_id",
                        column: x => x.support_id,
                        principalTable: "support",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_localization_history_workstation_workstation_id",
                        column: x => x.workstation_id,
                        principalTable: "workstation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workstation_allocation",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resource_id = table.Column<int>(type: "integer", nullable: false),
                    workstation_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workstation_allocation", x => x.id);
                    table.ForeignKey(
                        name: "FK_workstation_allocation_resource_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workstation_allocation_workstation_workstation_id",
                        column: x => x.workstation_id,
                        principalTable: "workstation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workstation_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    workstation_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workstation_status", x => x.id);
                    table.ForeignKey(
                        name: "FK_workstation_status_workstation_workstation_id",
                        column: x => x.workstation_id,
                        principalTable: "workstation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_config",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    config_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_config", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_config_config_config_id",
                        column: x => x.config_id,
                        principalTable: "config",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_config_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quality_check",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    manufacturing_phase_id = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_check", x => x.id);
                    table.ForeignKey(
                        name: "FK_quality_check_manufacturing_phase_manufacturing_phase_id",
                        column: x => x.manufacturing_phase_id,
                        principalTable: "manufacturing_phase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quality_check_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supported_product",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    support_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: true),
                    datetime_ini = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    datetime_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supported_product", x => x.id);
                    table.ForeignKey(
                        name: "FK_supported_product_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supported_product_support_support_id",
                        column: x => x.support_id,
                        principalTable: "support",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_phase",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notes = table.Column<string>(type: "text", nullable: true),
                    result = table.Column<string>(type: "text", nullable: true),
                    condition = table.Column<string>(type: "text", nullable: true),
                    datetime_ini = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    datetime_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    manufacturing_phase_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    workstation_id = table.Column<int>(type: "integer", nullable: false),
                    quality_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_phase", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_phase_manufacturing_phase_manufacturing_phase_id",
                        column: x => x.manufacturing_phase_id,
                        principalTable: "manufacturing_phase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_phase_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_phase_quality_check_quality_id",
                        column: x => x.quality_id,
                        principalTable: "quality_check",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_product_phase_workstation_workstation_id",
                        column: x => x.workstation_id,
                        principalTable: "workstation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_config_model_id",
                table: "config",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_localization_history_support_id",
                table: "localization_history",
                column: "support_id");

            migrationBuilder.CreateIndex(
                name: "IX_localization_history_workstation_id",
                table: "localization_history",
                column: "workstation_id");

            migrationBuilder.CreateIndex(
                name: "IX_manufacturing_order_client_order_id",
                table: "manufacturing_order",
                column: "client_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_material_manufacturing_phase_id",
                table: "model_material",
                column: "manufacturing_phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_material_material_id",
                table: "model_material",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_material_model_id",
                table: "model_material",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_phase_sequence_manufacturing_phase_id",
                table: "phase_sequence",
                column: "manufacturing_phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_phase_sequence_model_id",
                table: "phase_sequence",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_manufacturing_order_id",
                table: "product",
                column: "manufacturing_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_model_id",
                table: "product",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_config_config_id",
                table: "product_config",
                column: "config_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_config_product_id",
                table: "product_config",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_phase_manufacturing_phase_id",
                table: "product_phase",
                column: "manufacturing_phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_phase_product_id",
                table: "product_phase",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_phase_quality_id",
                table: "product_phase",
                column: "quality_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_phase_workstation_id",
                table: "product_phase",
                column: "workstation_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_check_manufacturing_phase_id",
                table: "quality_check",
                column: "manufacturing_phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_check_product_id",
                table: "quality_check",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_production_line_id",
                table: "support",
                column: "production_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_supported_product_product_id",
                table: "supported_product",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_supported_product_support_id",
                table: "supported_product",
                column: "support_id");

            migrationBuilder.CreateIndex(
                name: "IX_workstation_production_line_id",
                table: "workstation",
                column: "production_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_workstation_allocation_resource_id",
                table: "workstation_allocation",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "IX_workstation_allocation_workstation_id",
                table: "workstation_allocation",
                column: "workstation_id");

            migrationBuilder.CreateIndex(
                name: "IX_workstation_status_workstation_id",
                table: "workstation_status",
                column: "workstation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "localization_history");

            migrationBuilder.DropTable(
                name: "model_material");

            migrationBuilder.DropTable(
                name: "phase_sequence");

            migrationBuilder.DropTable(
                name: "product_config");

            migrationBuilder.DropTable(
                name: "product_phase");

            migrationBuilder.DropTable(
                name: "supported_product");

            migrationBuilder.DropTable(
                name: "workstation_allocation");

            migrationBuilder.DropTable(
                name: "workstation_status");

            migrationBuilder.DropTable(
                name: "material");

            migrationBuilder.DropTable(
                name: "config");

            migrationBuilder.DropTable(
                name: "quality_check");

            migrationBuilder.DropTable(
                name: "support");

            migrationBuilder.DropTable(
                name: "resource");

            migrationBuilder.DropTable(
                name: "workstation");

            migrationBuilder.DropTable(
                name: "manufacturing_phase");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "production_line");

            migrationBuilder.DropTable(
                name: "manufacturing_order");

            migrationBuilder.DropTable(
                name: "model");

            migrationBuilder.DropTable(
                name: "client_order");
        }
    }
}
