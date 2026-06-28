using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Drivolution.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_config_config_config_id",
                table: "product_config");

            migrationBuilder.DropColumn(
                name: "condition",
                table: "product_phase");

            migrationBuilder.DropColumn(
                name: "value",
                table: "product_config");

            migrationBuilder.DropColumn(
                name: "color_code",
                table: "product");

            migrationBuilder.DropColumn(
                name: "default_value",
                table: "config");

            migrationBuilder.RenameColumn(
                name: "config_id",
                table: "product_config",
                newName: "config_option_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_config_config_id",
                table: "product_config",
                newName: "IX_product_config_config_option_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "timestamp",
                table: "workstation_status",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "workstation_allocation",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "workstation_allocation",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "kind",
                table: "workstation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "manufacturing_phase_id",
                table: "workstation",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_ini",
                table: "supported_product",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_end",
                table: "supported_product",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "severity",
                table: "quality_check",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_ini",
                table: "product_phase",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_end",
                table: "product_phase",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "production_date",
                table: "product",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "max_acceptable_severity",
                table: "manufacturing_phase",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "rework_severity",
                table: "manufacturing_phase",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "time_threshold_pct",
                table: "manufacturing_phase",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "manufacturing_order",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "manufacturing_order",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_ini",
                table: "localization_history",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_end",
                table: "localization_history",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "allow_multiple",
                table: "config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "order_date",
                table: "client_order",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "client_order",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "alert",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    product_phase_id = table.Column<int>(type: "integer", nullable: false),
                    triggered_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    product_serial = table.Column<string>(type: "text", nullable: false),
                    phase_name = table.Column<string>(type: "text", nullable: false),
                    threshold_pct = table.Column<int>(type: "integer", nullable: true),
                    estimated_duration = table.Column<int>(type: "integer", nullable: true),
                    order_from = table.Column<int>(type: "integer", nullable: true),
                    order_to = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert", x => x.id);
                    table.ForeignKey(
                        name: "FK_alert_product_phase_product_phase_id",
                        column: x => x.product_phase_id,
                        principalTable: "product_phase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_alert_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    entity = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<int>(type: "integer", nullable: false),
                    entity_label = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "config_option",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    config_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_config_option", x => x.id);
                    table.ForeignKey(
                        name: "FK_config_option_config_config_id",
                        column: x => x.config_id,
                        principalTable: "config",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workstation_presence",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    app_user_id = table.Column<int>(type: "integer", nullable: false),
                    workstation_id = table.Column<int>(type: "integer", nullable: false),
                    checked_in_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    checked_out_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workstation_presence", x => x.id);
                    table.ForeignKey(
                        name: "FK_workstation_presence_app_user_app_user_id",
                        column: x => x.app_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workstation_presence_workstation_workstation_id",
                        column: x => x.workstation_id,
                        principalTable: "workstation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "phase_time_coefficient",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    manufacturing_phase_id = table.Column<int>(type: "integer", nullable: false),
                    config_option_id = table.Column<int>(type: "integer", nullable: true),
                    production_line_id = table.Column<int>(type: "integer", nullable: true),
                    model_id = table.Column<int>(type: "integer", nullable: true),
                    weight_seconds = table.Column<decimal>(type: "numeric", nullable: false),
                    trained_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phase_time_coefficient", x => x.id);
                    table.ForeignKey(
                        name: "FK_phase_time_coefficient_config_option_config_option_id",
                        column: x => x.config_option_id,
                        principalTable: "config_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_phase_time_coefficient_manufacturing_phase_manufacturing_ph~",
                        column: x => x.manufacturing_phase_id,
                        principalTable: "manufacturing_phase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_phase_time_coefficient_model_model_id",
                        column: x => x.model_id,
                        principalTable: "model",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_phase_time_coefficient_production_line_production_line_id",
                        column: x => x.production_line_id,
                        principalTable: "production_line",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workstation_manufacturing_phase_id",
                table: "workstation",
                column: "manufacturing_phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_alert_product_id",
                table: "alert",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_alert_product_phase_id",
                table: "alert",
                column: "product_phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_user_email",
                table: "app_user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_config_option_config_id",
                table: "config_option",
                column: "config_id");

            migrationBuilder.CreateIndex(
                name: "IX_phase_time_coefficient_config_option_id",
                table: "phase_time_coefficient",
                column: "config_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_phase_time_coefficient_manufacturing_phase_id",
                table: "phase_time_coefficient",
                column: "manufacturing_phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_phase_time_coefficient_model_id",
                table: "phase_time_coefficient",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_phase_time_coefficient_production_line_id",
                table: "phase_time_coefficient",
                column: "production_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_workstation_presence_app_user_id",
                table: "workstation_presence",
                column: "app_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_workstation_presence_workstation_id",
                table: "workstation_presence",
                column: "workstation_id");

            migrationBuilder.AddForeignKey(
                name: "FK_product_config_config_option_config_option_id",
                table: "product_config",
                column: "config_option_id",
                principalTable: "config_option",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workstation_manufacturing_phase_manufacturing_phase_id",
                table: "workstation",
                column: "manufacturing_phase_id",
                principalTable: "manufacturing_phase",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_config_config_option_config_option_id",
                table: "product_config");

            migrationBuilder.DropForeignKey(
                name: "FK_workstation_manufacturing_phase_manufacturing_phase_id",
                table: "workstation");

            migrationBuilder.DropTable(
                name: "alert");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "phase_time_coefficient");

            migrationBuilder.DropTable(
                name: "workstation_presence");

            migrationBuilder.DropTable(
                name: "config_option");

            migrationBuilder.DropTable(
                name: "app_user");

            migrationBuilder.DropIndex(
                name: "IX_workstation_manufacturing_phase_id",
                table: "workstation");

            migrationBuilder.DropColumn(
                name: "kind",
                table: "workstation");

            migrationBuilder.DropColumn(
                name: "manufacturing_phase_id",
                table: "workstation");

            migrationBuilder.DropColumn(
                name: "severity",
                table: "quality_check");

            migrationBuilder.DropColumn(
                name: "max_acceptable_severity",
                table: "manufacturing_phase");

            migrationBuilder.DropColumn(
                name: "rework_severity",
                table: "manufacturing_phase");

            migrationBuilder.DropColumn(
                name: "time_threshold_pct",
                table: "manufacturing_phase");

            migrationBuilder.DropColumn(
                name: "allow_multiple",
                table: "config");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "client_order");

            migrationBuilder.RenameColumn(
                name: "config_option_id",
                table: "product_config",
                newName: "config_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_config_config_option_id",
                table: "product_config",
                newName: "IX_product_config_config_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "timestamp",
                table: "workstation_status",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "workstation_allocation",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "workstation_allocation",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_ini",
                table: "supported_product",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_end",
                table: "supported_product",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_ini",
                table: "product_phase",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_end",
                table: "product_phase",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "condition",
                table: "product_phase",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "value",
                table: "product_config",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "production_date",
                table: "product",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "color_code",
                table: "product",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "manufacturing_order",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                table: "manufacturing_order",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_ini",
                table: "localization_history",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "datetime_end",
                table: "localization_history",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "default_value",
                table: "config",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "order_date",
                table: "client_order",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_product_config_config_config_id",
                table: "product_config",
                column: "config_id",
                principalTable: "config",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
