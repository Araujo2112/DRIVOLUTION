using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiTexPact.Migrations
{
    /// <inheritdoc />
    public partial class CreatingModelToAIWithTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Prediction",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Prediction");
        }
    }
}
