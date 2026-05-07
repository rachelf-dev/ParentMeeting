using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataContext.Migrations
{
    /// <inheritdoc />
    public partial class DisconnectAvailabilityFromParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentAvailability_Parents_ParentId",
                table: "ParentAvailability");

            migrationBuilder.AlterColumn<string>(
                name: "ParentIdentity",
                table: "ParentAvailability",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentAvailability_Parents_ParentId",
                table: "ParentAvailability",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentAvailability_Parents_ParentId",
                table: "ParentAvailability");

            migrationBuilder.AlterColumn<string>(
                name: "ParentIdentity",
                table: "ParentAvailability",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(12)",
                oldMaxLength: 12);

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
