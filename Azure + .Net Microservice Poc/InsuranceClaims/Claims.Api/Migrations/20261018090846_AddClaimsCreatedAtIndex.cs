using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claims.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimsCreatedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Claims_CreatedAtUtc_Id",
                table: "Claims",
                columns: new[] { "CreatedAtUtc", "Id" },
                descending: new[] { true, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Claims_CreatedAtUtc_Id",
                table: "Claims");
        }
    }
}
