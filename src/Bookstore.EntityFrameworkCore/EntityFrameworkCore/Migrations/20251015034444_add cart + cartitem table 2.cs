using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.Migrations
{
    /// <inheritdoc />
    public partial class addcartcartitemtable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_BookEditions_EditionId",
                table: "CartItems");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_BookEditions_EditionId",
                table: "CartItems",
                column: "EditionId",
                principalTable: "BookEditions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_BookEditions_EditionId",
                table: "CartItems");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_BookEditions_EditionId",
                table: "CartItems",
                column: "EditionId",
                principalTable: "BookEditions",
                principalColumn: "Id");
        }
    }
}
