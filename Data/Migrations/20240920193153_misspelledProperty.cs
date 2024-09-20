using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentsApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class misspelledProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "Payments",
                newName: "Quantity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Payments",
                newName: "Quality");
        }
    }
}
