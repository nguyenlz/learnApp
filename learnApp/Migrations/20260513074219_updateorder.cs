using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learnApp.Migrations
{
    /// <inheritdoc />
    public partial class updateorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Sites_SiteId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "SiteId",
                table: "Orders",
                newName: "SiteID");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_SiteId",
                table: "Orders",
                newName: "IX_Orders_SiteID");

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Orders",
                type: "decimal(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "OrderDetails",
                type: "decimal(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "OrderDetails",
                type: "decimal(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Site",
                table: "Orders",
                column: "SiteID",
                principalTable: "Sites",
                principalColumn: "SiteId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Site",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "SiteID",
                table: "Orders",
                newName: "SiteId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_SiteID",
                table: "Orders",
                newName: "IX_Orders_SiteId");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "OrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,2)",
                oldPrecision: 14,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "OrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(14,2)",
                oldPrecision: 14,
                oldScale: 2);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Sites_SiteId",
                table: "Orders",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "SiteId");
        }
    }
}
