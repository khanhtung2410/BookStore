using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.Migrations
{
    /// <inheritdoc />
    public partial class adddb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInventories_Books_BookId",
                table: "BookInventories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookInventories",
                table: "BookInventories");

            migrationBuilder.DropIndex(
                name: "IX_BookInventories_BookId",
                table: "BookInventories");

            migrationBuilder.DropColumn(
                name: "EditionName",
                table: "BookEditions");

            migrationBuilder.RenameTable(
                name: "BookInventories",
                newName: "BookInvantories");

            migrationBuilder.RenameColumn(
                name: "BookId",
                table: "BookInvantories",
                newName: "BookEditionId");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "BookInvantories",
                newName: "StockQuantity");

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

            migrationBuilder.CreateIndex(
                name: "IX_BookInvantories_BookEditionId",
                table: "BookInvantories",
                column: "BookEditionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookInvantories_BookEditions_BookEditionId",
                table: "BookInvantories",
                column: "BookEditionId",
                principalTable: "BookEditions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInvantories_BookEditions_BookEditionId",
                table: "BookInvantories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookInvantories",
                table: "BookInvantories");

            migrationBuilder.DropIndex(
                name: "IX_BookInvantories_BookEditionId",
                table: "BookInvantories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BookInvantories");

            migrationBuilder.RenameTable(
                name: "BookInvantories",
                newName: "BookInventories");

            migrationBuilder.RenameColumn(
                name: "StockQuantity",
                table: "BookInventories",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "BookEditionId",
                table: "BookInventories",
                newName: "BookId");

            migrationBuilder.AddColumn<string>(
                name: "EditionName",
                table: "BookEditions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookInventories",
                table: "BookInventories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BookInventories_BookId",
                table: "BookInventories",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookInventories_Books_BookId",
                table: "BookInventories",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
