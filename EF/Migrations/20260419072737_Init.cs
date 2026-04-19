using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TollInvoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromDay = table.Column<DateOnly>(type: "date", nullable: false),
                    ToDay = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollInvoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyTollSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ForDay = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TollInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyTollSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyTollSummaries_TollInvoices_TollInvoiceId",
                        column: x => x.TollInvoiceId,
                        principalTable: "TollInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DailyTollSummaries_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TollEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DailyTollSummaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TollEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TollEvents_DailyTollSummaries_DailyTollSummaryId",
                        column: x => x.DailyTollSummaryId,
                        principalTable: "DailyTollSummaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TollEvents_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyTollSummaries_TollInvoiceId",
                table: "DailyTollSummaries",
                column: "TollInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTollSummaries_VehicleId_ForDay",
                table: "DailyTollSummaries",
                columns: new[] { "VehicleId", "ForDay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TollEvents_DailyTollSummaryId",
                table: "TollEvents",
                column: "DailyTollSummaryId");

            migrationBuilder.CreateIndex(
                name: "IX_TollEvents_VehicleId",
                table: "TollEvents",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_RegistrationNumber",
                table: "Vehicles",
                column: "RegistrationNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TollEvents");

            migrationBuilder.DropTable(
                name: "DailyTollSummaries");

            migrationBuilder.DropTable(
                name: "TollInvoices");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
