using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claims.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimDeploymentNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeploymentNotes",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "DeploymentNotes",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeploymentNotes",
                table: "Claims");
        }
    }
}
