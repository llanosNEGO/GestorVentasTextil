using System;
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
            migrationBuilder.RenameColumn(
                name: "precioUnitario",
                table: "DetallePedido",
                newName: "precio_unitario");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_registro",
                table: "Ventas",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "precio_unitario",
                table: "DetallePedido",
                newName: "precioUnitario");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "fecha_registro",
                table: "Ventas",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
