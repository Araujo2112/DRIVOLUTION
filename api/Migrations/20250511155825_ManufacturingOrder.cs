using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTexPact.Migrations
{
    /// <inheritdoc />
    public partial class ManufacturingOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhaseInfo",
                table: "ManufacturingOrders",
                newName: "Observations");

            migrationBuilder.RenameColumn(
                name: "PhaseDuration",
                table: "ManufacturingOrders",
                newName: "OrderNumber");

            migrationBuilder.AddColumn<DateTime>(
                name: "SheduleInit",
                table: "ManufacturingOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SheduleInit",
                table: "ManufacturingOrders");

            migrationBuilder.RenameColumn(
                name: "OrderNumber",
                table: "ManufacturingOrders",
                newName: "PhaseDuration");

            migrationBuilder.RenameColumn(
                name: "Observations",
                table: "ManufacturingOrders",
                newName: "PhaseInfo");
        }
    }
}
