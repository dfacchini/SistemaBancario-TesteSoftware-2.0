using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaBancario.Migrations
{
    /// <inheritdoc />
    public partial class SaldoInicialContas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContasCorrentes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Saldo",
                value: 1000m);

            migrationBuilder.UpdateData(
                table: "ContasCorrentes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Saldo",
                value: 1000m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContasCorrentes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Saldo",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "ContasCorrentes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Saldo",
                value: 0m);
        }
    }
}
