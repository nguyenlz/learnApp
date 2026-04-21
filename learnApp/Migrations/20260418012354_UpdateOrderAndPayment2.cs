using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learnApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderAndPayment2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Site_SiteId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Site_Customers_CustomerId",
                table: "Site");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Site",
                table: "Site");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "Site",
                newName: "Sites");

            migrationBuilder.RenameIndex(
                name: "IX_Site_CustomerId",
                table: "Sites",
                newName: "IX_Sites_CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sites",
                table: "Sites",
                column: "SiteId");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Sites_SiteId",
                table: "Orders",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Customers_CustomerId",
                table: "Sites",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Sites_SiteId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Customers_CustomerId",
                table: "Sites");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sites",
                table: "Sites");

            migrationBuilder.RenameTable(
                name: "Sites",
                newName: "Site");

            migrationBuilder.RenameIndex(
                name: "IX_Sites_CustomerId",
                table: "Site",
                newName: "IX_Site_CustomerId");

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Site",
                table: "Site",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Site_SiteId",
                table: "Orders",
                column: "SiteId",
                principalTable: "Site",
                principalColumn: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Site_Customers_CustomerId",
                table: "Site",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
