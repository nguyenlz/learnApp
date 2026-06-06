using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace learnApp.Migrations
{
    /// <inheritdoc />
    public partial class updateStockImportDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__StockImp__4DD7ABE466E9BCBD",
                table: "StockImportDetails");

            migrationBuilder.AddColumn<int>(
                name: "ImportDetailId",
                table: "StockImportDetails",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockImportDetails",
                table: "StockImportDetails",
                column: "ImportDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_StockImportDetails_ImportID",
                table: "StockImportDetails",
                column: "ImportID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StockImportDetails",
                table: "StockImportDetails");

            migrationBuilder.DropIndex(
                name: "IX_StockImportDetails_ImportID",
                table: "StockImportDetails");

            migrationBuilder.DropColumn(
                name: "ImportDetailId",
                table: "StockImportDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK__StockImp__4DD7ABE466E9BCBD",
                table: "StockImportDetails",
                columns: new[] { "ImportID", "ProductID" });
        }
    }
}
