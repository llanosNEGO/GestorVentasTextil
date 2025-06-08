using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoFinal.Migrations
{
    /// <inheritdoc />
    public partial class _4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "subtotal",
                table: "DetalleCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MedioPago",
                table: "ComprasProveedores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "idmedioPago",
                table: "ComprasProveedores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ComprasProveedores_idmedioPago",
                table: "ComprasProveedores",
                column: "idmedioPago");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasProveedores_MedioPago_idmedioPago",
                table: "ComprasProveedores",
                column: "idmedioPago",
                principalTable: "MedioPago",
                principalColumn: "idMedioPago",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComprasProveedores_MedioPago_idmedioPago",
                table: "ComprasProveedores");

            migrationBuilder.DropIndex(
                name: "IX_ComprasProveedores_idmedioPago",
                table: "ComprasProveedores");

            migrationBuilder.DropColumn(
                name: "subtotal",
                table: "DetalleCompra");

            migrationBuilder.DropColumn(
                name: "MedioPago",
                table: "ComprasProveedores");

            migrationBuilder.DropColumn(
                name: "idmedioPago",
                table: "ComprasProveedores");
        }
    }
}
