using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmIctm.Api.Infraestrutura.Migracoes
{
    /// <inheritdoc />
    public partial class CadastroMembros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentificadorPublicoMembros",
                schema: "crm",
                table: "igrejas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // Cada igreja existente precisa de um endereço independente do QR de visitantes.
            // UUID v4 fornece um identificador aleatório sem extensão extra do PostgreSQL.
            migrationBuilder.Sql("""
                UPDATE crm.igrejas
                SET "IdentificadorPublicoMembros" = replace(gen_random_uuid()::text, '-', '')
                WHERE "IdentificadorPublicoMembros" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "IdentificadorPublicoMembros",
                schema: "crm",
                table: "igrejas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "historicos_membros",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IgrejaId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembroId = table.Column<Guid>(type: "uuid", nullable: true),
                    SolicitacaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Acao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historicos_membros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_historicos_membros_usuarios_AutorId",
                        column: x => x.AutorId,
                        principalSchema: "crm",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membros",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IgrejaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Dados_Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Dados_Sobrenome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Dados_Whatsapp = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    Dados_Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    Dados_DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Dados_Logradouro = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Dados_Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Dados_Bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Dados_Complemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Dados_Cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Dados_Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Dados_Estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Dados_SituacaoBatismo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Dados_DataBatismo = table.Column<DateOnly>(type: "date", nullable: true),
                    Dados_EhMenor = table.Column<bool>(type: "boolean", nullable: false),
                    Dados_NomeResponsavelLegal = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Dados_WhatsappResponsavelLegal = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    Dados_VinculoResponsavelLegal = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Situacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Origem = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DataIngresso = table.Column<DateOnly>(type: "date", nullable: true),
                    ObservacaoPastoral = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MotivoInativacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_membros_igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalSchema: "crm",
                        principalTable: "igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitacoes_membros",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IgrejaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Dados_Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Dados_Sobrenome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Dados_Whatsapp = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    Dados_Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    Dados_DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Dados_Logradouro = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Dados_Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Dados_Bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Dados_Complemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Dados_Cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Dados_Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Dados_Estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Dados_SituacaoBatismo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Dados_DataBatismo = table.Column<DateOnly>(type: "date", nullable: true),
                    Dados_EhMenor = table.Column<bool>(type: "boolean", nullable: false),
                    Dados_NomeResponsavelLegal = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Dados_WhatsappResponsavelLegal = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    Dados_VinculoResponsavelLegal = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AvisoPrivacidadeVersao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CriadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecididaPorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecididaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MotivoRecusa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MembroId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitacoes_membros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_solicitacoes_membros_igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalSchema: "crm",
                        principalTable: "igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_solicitacoes_membros_usuarios_DecididaPorId",
                        column: x => x.DecididaPorId,
                        principalSchema: "crm",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_igrejas_IdentificadorPublicoMembros",
                schema: "crm",
                table: "igrejas",
                column: "IdentificadorPublicoMembros",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_historicos_membros_AutorId",
                schema: "crm",
                table: "historicos_membros",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_historicos_membros_IgrejaId_MembroId_CriadoEm",
                schema: "crm",
                table: "historicos_membros",
                columns: new[] { "IgrejaId", "MembroId", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_historicos_membros_IgrejaId_SolicitacaoId_CriadoEm",
                schema: "crm",
                table: "historicos_membros",
                columns: new[] { "IgrejaId", "SolicitacaoId", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_membros_IgrejaId_Situacao",
                schema: "crm",
                table: "membros",
                columns: new[] { "IgrejaId", "Situacao" });

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_membros_DecididaPorId",
                schema: "crm",
                table: "solicitacoes_membros",
                column: "DecididaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_membros_IgrejaId_Estado_CriadaEm",
                schema: "crm",
                table: "solicitacoes_membros",
                columns: new[] { "IgrejaId", "Estado", "CriadaEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historicos_membros",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "membros",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "solicitacoes_membros",
                schema: "crm");

            migrationBuilder.DropIndex(
                name: "IX_igrejas_IdentificadorPublicoMembros",
                schema: "crm",
                table: "igrejas");

            migrationBuilder.DropColumn(
                name: "IdentificadorPublicoMembros",
                schema: "crm",
                table: "igrejas");
        }
    }
}
