using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CrmIctm.Api.Infraestrutura.Migracoes
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "crm");

            migrationBuilder.CreateTable(
                name: "igrejas",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    IdentificadorPublico = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CriadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_igrejas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "perfis_identity",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perfis_identity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Perfil = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IgrejaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuarios_igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalSchema: "crm",
                        principalTable: "igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "perfis_claims",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perfis_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_perfis_claims_perfis_identity_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "crm",
                        principalTable: "perfis_identity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "atendimentos",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IgrejaId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeVisitante = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Whatsapp = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    QuerConhecerIgreja = table.Column<bool>(type: "boolean", nullable: false),
                    QuerConversaOracao = table.Column<bool>(type: "boolean", nullable: false),
                    Etapa = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ResponsavelId = table.Column<Guid>(type: "uuid", nullable: true),
                    AvisoPrivacidadeVersao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Resultado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ConcluidoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UltimaAtividadeEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atendimentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_atendimentos_igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalSchema: "crm",
                        principalTable: "igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_atendimentos_usuarios_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalSchema: "crm",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_claims",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuarios_claims_usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "crm",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_logins",
                schema: "crm",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_logins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_usuarios_logins_usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "crm",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_perfis_identity",
                schema: "crm",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_perfis_identity", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_usuarios_perfis_identity_perfis_identity_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "crm",
                        principalTable: "perfis_identity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuarios_perfis_identity_usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "crm",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_tokens",
                schema: "crm",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_usuarios_tokens_usuarios_UserId",
                        column: x => x.UserId,
                        principalSchema: "crm",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "historicos_atendimento",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AtendimentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historicos_atendimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_historicos_atendimento_atendimentos_AtendimentoId",
                        column: x => x.AtendimentoId,
                        principalSchema: "crm",
                        principalTable: "atendimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_historicos_atendimento_usuarios_AutorId",
                        column: x => x.AutorId,
                        principalSchema: "crm",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "observacoes_atendimento",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AtendimentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CriadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_observacoes_atendimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_observacoes_atendimento_atendimentos_AtendimentoId",
                        column: x => x.AtendimentoId,
                        principalSchema: "crm",
                        principalTable: "atendimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_observacoes_atendimento_usuarios_AutorId",
                        column: x => x.AutorId,
                        principalSchema: "crm",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_atendimentos_IgrejaId_Etapa",
                schema: "crm",
                table: "atendimentos",
                columns: new[] { "IgrejaId", "Etapa" });

            migrationBuilder.CreateIndex(
                name: "IX_atendimentos_IgrejaId_Whatsapp",
                schema: "crm",
                table: "atendimentos",
                columns: new[] { "IgrejaId", "Whatsapp" });

            migrationBuilder.CreateIndex(
                name: "IX_atendimentos_ResponsavelId",
                schema: "crm",
                table: "atendimentos",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_historicos_atendimento_AtendimentoId",
                schema: "crm",
                table: "historicos_atendimento",
                column: "AtendimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_historicos_atendimento_AutorId",
                schema: "crm",
                table: "historicos_atendimento",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_igrejas_IdentificadorPublico",
                schema: "crm",
                table: "igrejas",
                column: "IdentificadorPublico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_observacoes_atendimento_AtendimentoId",
                schema: "crm",
                table: "observacoes_atendimento",
                column: "AtendimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_observacoes_atendimento_AutorId",
                schema: "crm",
                table: "observacoes_atendimento",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_perfis_claims_RoleId",
                schema: "crm",
                table: "perfis_claims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "crm",
                table: "perfis_identity",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "crm",
                table: "usuarios",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_IgrejaId",
                schema: "crm",
                table: "usuarios",
                column: "IgrejaId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "crm",
                table: "usuarios",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_claims_UserId",
                schema: "crm",
                table: "usuarios_claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_logins_UserId",
                schema: "crm",
                table: "usuarios_logins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_perfis_identity_RoleId",
                schema: "crm",
                table: "usuarios_perfis_identity",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historicos_atendimento",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "observacoes_atendimento",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "perfis_claims",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "usuarios_claims",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "usuarios_logins",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "usuarios_perfis_identity",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "usuarios_tokens",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "atendimentos",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "perfis_identity",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "igrejas",
                schema: "crm");
        }
    }
}
