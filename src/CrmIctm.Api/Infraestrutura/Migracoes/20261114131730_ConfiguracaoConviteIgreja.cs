using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmIctm.Api.Infraestrutura.Migracoes
{
    /// <inheritdoc />
    public partial class ConfiguracaoConviteIgreja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CultoPrincipal",
                schema: "crm",
                table: "igrejas",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                schema: "crm",
                table: "igrejas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CultoPrincipal",
                schema: "crm",
                table: "igrejas");

            migrationBuilder.DropColumn(
                name: "Instagram",
                schema: "crm",
                table: "igrejas");
        }
    }
}
