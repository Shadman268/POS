using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiptType",
                table: "Receipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OriginalReceiptId",
                table: "Receipts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalReceiptItemId",
                table: "ReceiptItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_OriginalReceiptId",
                table: "Receipts",
                column: "OriginalReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_TenantId_OriginalReceiptId",
                table: "Receipts",
                columns: new[] { "TenantId", "OriginalReceiptId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Receipts_OriginalReceiptId",
                table: "Receipts",
                column: "OriginalReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Receipts_OriginalReceiptId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_OriginalReceiptId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_TenantId_OriginalReceiptId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "ReceiptType",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "OriginalReceiptId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "OriginalReceiptItemId",
                table: "ReceiptItems");
        }
    }
}
