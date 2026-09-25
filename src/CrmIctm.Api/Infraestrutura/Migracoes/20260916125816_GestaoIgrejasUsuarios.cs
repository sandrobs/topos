using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmIctm.Api.Infraestrutura.Migracoes
{
    /// <inheritdoc />
    public partial class GestaoIgrejasUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AtualizadoEm",
                schema: "crm",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "DeveTrocarSenha",
                schema: "crm",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE crm.usuarios
                SET "AtualizadoEm" = "CriadoEm";
                """);

            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                schema: "crm",
                table: "igrejas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cep",
                schema: "crm",
                table: "igrejas",
                type: "character(8)",
                fixedLength: true,
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                schema: "crm",
                table: "igrejas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                schema: "crm",
                table: "igrejas",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                schema: "crm",
                table: "igrejas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "registros_auditoria_administrativa",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoEntidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EntidadeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Acao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registros_auditoria_administrativa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_registros_auditoria_administrativa_usuarios_AutorId",
                        column: x => x.AutorId,
                        principalSchema: "crm",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_registros_auditoria_administrativa_AutorId",
                schema: "crm",
                table: "registros_auditoria_administrativa",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_registros_auditoria_administrativa_TipoEntidade_EntidadeId_~",
                schema: "crm",
                table: "registros_auditoria_administrativa",
                columns: new[] { "TipoEntidade", "EntidadeId", "CriadoEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registros_auditoria_administrativa",
                schema: "crm");

            migrationBuilder.DropColumn(
                name: "AtualizadoEm",
                schema: "crm",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "DeveTrocarSenha",
                schema: "crm",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Bairro",
                schema: "crm",
                table: "igrejas");

            migrationBuilder.DropColumn(
                name: "Cep",
                schema: "crm",
                table: "igrejas");

            migrationBuilder.DropColumn(
                name: "Complemento",
                schema: "crm",
                table: "igrejas");

            migrationBuilder.DropColumn(
                name: "Logradouro",
                schema: "crm",
                table: "igrejas");

            migrationBuilder.DropColumn(
                name: "Numero",
                schema: "crm",
                table: "igrejas");
        }
    }
}
