using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learnApp.Migrations
{
    /// <inheritdoc />
    public partial class CustomerPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerPayment",
                columns: table => new
                {
                    CustomerPaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPayment", x => x.CustomerPaymentId);
                    table.ForeignKey(
                        name: "FK_CustomerPayment_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerPayment_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "SiteId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPayment_CustomerID",
                table: "CustomerPayment",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPayment_SiteId",
                table: "CustomerPayment",
                column: "SiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerPayment");
        }
    }
}
