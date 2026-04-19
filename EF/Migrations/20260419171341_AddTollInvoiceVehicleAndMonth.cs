using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTollInvoiceVehicleAndMonth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromDay",
                table: "TollInvoices");

            migrationBuilder.DropColumn(
                name: "ToDay",
                table: "TollInvoices");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "TollInvoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "TollInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId",
                table: "TollInvoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "TollInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TollInvoices_VehicleId_Year_Month",
                table: "TollInvoices",
                columns: new[] { "VehicleId", "Year", "Month" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TollInvoices_Vehicles_VehicleId",
                table: "TollInvoices",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TollInvoices_Vehicles_VehicleId",
                table: "TollInvoices");

            migrationBuilder.DropIndex(
                name: "IX_TollInvoices_VehicleId_Year_Month",
                table: "TollInvoices");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "TollInvoices");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "TollInvoices");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "TollInvoices");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "TollInvoices");

            migrationBuilder.AddColumn<DateOnly>(
                name: "FromDay",
                table: "TollInvoices",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "ToDay",
                table: "TollInvoices",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}
