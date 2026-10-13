using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastFoodAPI.Migrations
{
    /// <inheritdoc />
    public partial class OrderCounterByArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UltimoNumeroPedido",
                table: "Areas",
                newName: "LastOrderNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastOrderNumber",
                table: "Areas",
                newName: "UltimoNumeroPedido");
        }
    }
}
