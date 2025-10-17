using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.Migrations
{
    /// <inheritdoc />
    public partial class fixinventorytablename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInvantories_BookEditions_BookEditionId",
                table: "BookInvantories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookInvantories",
                table: "BookInvantories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BookInvantories");

            migrationBuilder.RenameTable(
                name: "BookInvantories",
                newName: "BookInventories");

            migrationBuilder.RenameIndex(
                name: "IX_BookInvantories_BookEditionId",
                table: "BookInventories",
                newName: "IX_BookInventories_BookEditionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookInventories",
                table: "BookInventories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookInventories_BookEditions_BookEditionId",
                table: "BookInventories",
                column: "BookEditionId",
                principalTable: "BookEditions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInventories_BookEditions_BookEditionId",
                table: "BookInventories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookInventories",
                table: "BookInventories");

            migrationBuilder.RenameTable(
                name: "BookInventories",
                newName: "BookInvantories");

            migrationBuilder.RenameIndex(
                name: "IX_BookInventories_BookEditionId",
                table: "BookInvantories",
                newName: "IX_BookInvantories_BookEditionId");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BookInvantories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookInvantories",
                table: "BookInvantories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookInvantories_BookEditions_BookEditionId",
                table: "BookInvantories",
                column: "BookEditionId",
                principalTable: "BookEditions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
