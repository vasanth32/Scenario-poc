using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Policy.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HolderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CoverageAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Policies",
                columns: new[] { "Id", "CoverageAmount", "EndDate", "HolderName", "IsActive", "PolicyNumber", "StartDate" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 250000m, new DateOnly(2026, 12, 31), "Alice Johnson", true, "POL-1001", new DateOnly(2025, 1, 1) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 100000m, new DateOnly(2024, 5, 31), "Bob Smith", false, "POL-1002", new DateOnly(2023, 6, 1) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PolicyNumber",
                table: "Policies",
                column: "PolicyNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Policies");
        }
    }
}
