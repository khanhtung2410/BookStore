using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.Migrations
{
    /// <inheritdoc />
    public partial class changeEditionIdtoBookEditionIdincartitemtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_BookEditions_EditionId",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "EditionId",
                table: "CartItems",
                newName: "BookEditionId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_EditionId",
                table: "CartItems",
                newName: "IX_CartItems_BookEditionId");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Carts",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_BookEditions_BookEditionId",
                table: "CartItems",
                column: "BookEditionId",
                principalTable: "BookEditions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_BookEditions_BookEditionId",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "BookEditionId",
                table: "CartItems",
                newName: "EditionId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_BookEditionId",
                table: "CartItems",
                newName: "IX_CartItems_EditionId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Carts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_BookEditions_EditionId",
                table: "CartItems",
                column: "EditionId",
                principalTable: "BookEditions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
