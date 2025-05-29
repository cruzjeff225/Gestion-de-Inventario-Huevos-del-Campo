using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestión_de_Inventario_Huevos_del_Campo.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCampoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "Ventas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "Ventas");
        }
    }
}
