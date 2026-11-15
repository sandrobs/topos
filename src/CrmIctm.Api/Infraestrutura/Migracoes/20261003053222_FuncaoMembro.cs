using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmIctm.Api.Infraestrutura.Migracoes
{
    /// <inheritdoc />
    public partial class FuncaoMembro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dados_Funcao",
                schema: "crm",
                table: "solicitacoes_membros",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "MEMBRO");

            migrationBuilder.AddColumn<string>(
                name: "Dados_Funcao",
                schema: "crm",
                table: "membros",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "MEMBRO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dados_Funcao",
                schema: "crm",
                table: "solicitacoes_membros");

            migrationBuilder.DropColumn(
                name: "Dados_Funcao",
                schema: "crm",
                table: "membros");
        }
    }
}
