using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learnApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderID",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "OrderID",
                table: "Payments",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_OrderID",
                table: "Payments",
                newName: "IX_Payments_OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Payments",
                newName: "OrderID");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                newName: "IX_Payments_OrderID");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderID",
                table: "Payments",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
