using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataContext.Migrations
{
    /// <inheritdoc />
    public partial class MakeParentIdOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentAvailability_Parents_ParentId",
                table: "ParentAvailability");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentAvailability_Parents_ParentId",
                table: "ParentAvailability",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentAvailability_Parents_ParentId",
                table: "ParentAvailability");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentAvailability_Parents_ParentId",
                table: "ParentAvailability",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
