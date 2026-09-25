using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmIctm.Api.Infraestrutura.Migracoes
{
    /// <inheritdoc />
    public partial class DadosComplementaresAtendimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BairroVisitante",
                schema: "crm",
                table: "atendimentos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplementoEndereco",
                schema: "crm",
                table: "atendimentos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataNascimento",
                schema: "crm",
                table: "atendimentos",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogradouroVisitante",
                schema: "crm",
                table: "atendimentos",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeIgrejaCongrega",
                schema: "crm",
                table: "atendimentos",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroResidencia",
                schema: "crm",
                table: "atendimentos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SituacaoCongregacional",
                schema: "crm",
                table: "atendimentos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "NaoInformado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BairroVisitante",
                schema: "crm",
                table: "atendimentos");

            migrationBuilder.DropColumn(
                name: "ComplementoEndereco",
                schema: "crm",
                table: "atendimentos");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                schema: "crm",
                table: "atendimentos");

            migrationBuilder.DropColumn(
                name: "LogradouroVisitante",
                schema: "crm",
                table: "atendimentos");

            migrationBuilder.DropColumn(
                name: "NomeIgrejaCongrega",
                schema: "crm",
                table: "atendimentos");

            migrationBuilder.DropColumn(
                name: "NumeroResidencia",
                schema: "crm",
                table: "atendimentos");

            migrationBuilder.DropColumn(
                name: "SituacaoCongregacional",
                schema: "crm",
                table: "atendimentos");
        }
    }
}
