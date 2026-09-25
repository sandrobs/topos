using System.Security.Cryptography;
using CrmIctm.Api.Contratos;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Infraestrutura;
using CrmIctm.Api.Servicos;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CrmIctm.Api.Endpoints;

public static class UsuariosEndpoints
{
    private static readonly string[] PalavrasSenha =
    [
        "Abrigo", "Alegria", "Amparo", "Aurora", "Caminho", "Cedro", "Cuidado", "Esperanca",
        "Estrela", "Familia", "Firmeza", "Flores", "Fonte", "Graca", "Harmonia", "Jardim",
        "Luz", "Manha", "Monte", "Oliveira", "Paz", "Ponte", "Porto", "Presenca",
        "Renovo", "Rocha", "Semente", "Sereno", "Sol", "Uniao", "Vida", "Vitoria"
    ];

    private const string CaracteresSenha = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static IEndpointRouteBuilder MapearUsuarios(this IEndpointRouteBuilder endpoints)
    {
        var grupo = endpoints.MapGroup("/api/usuarios")
            .WithTags("Usuários")
            .RequireAuthorization();

        grupo.MapGet("", ListarAtivosAsync);
        grupo.MapGet("/gestao", ListarParaGestaoAsync);
        grupo.MapGet("/senha-sugerida", SugerirSenhaAsync);
        grupo.MapPost("", CriarAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        grupo.MapPut("/{id:guid}", EditarAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        grupo.MapPut("/{id:guid}/situacao", AlterarSituacaoAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        grupo.MapPost("/{id:guid}/redefinir-senha", RedefinirSenhaAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());

        return endpoints;
    }

    private static async Task<IResult> ListarAtivosAsync(
        Guid? igrejaId,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        var igrejaAutorizada = usuario.Perfil == PerfilUsuario.Administrador
            ? igrejaId
            : usuario.IgrejaId;

        if (igrejaAutorizada is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["igrejaId"] = ["Selecione uma igreja."]
            });
        }

        if (usuario.Perfil != PerfilUsuario.Administrador &&
            igrejaId.HasValue &&
            igrejaId != usuario.IgrejaId)
        {
            return Results.Forbid();
        }

        var usuarios = await banco.Users
            .AsNoTracking()
            .Where(x => x.Ativo && x.IgrejaId == igrejaAutorizada)
            .OrderBy(x => x.Nome)
            .Select(x => new { x.Id, x.Nome, Perfil = x.Perfil.ParaCodigo() })
            .ToListAsync(cancellationToken);

        return Results.Ok(usuarios);
    }

    private static async Task<IResult> ListarParaGestaoAsync(
        Guid? igrejaId,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        if (usuario.Perfil == PerfilUsuario.Equipe)
        {
            return Results.Forbid();
        }

        IQueryable<Usuario> consulta = banco.Users.AsNoTracking();
        if (usuario.Perfil == PerfilUsuario.Pastor)
        {
            if (igrejaId.HasValue && igrejaId != usuario.IgrejaId)
            {
                return Results.Forbid();
            }

            consulta = consulta.Where(x => x.IgrejaId == usuario.IgrejaId);
        }
        else if (igrejaId.HasValue)
        {
            consulta = consulta.Where(x => x.IgrejaId == igrejaId);
        }
        else
        {
            consulta = consulta.Where(x => x.Perfil == PerfilUsuario.Administrador);
        }

        var usuarios = await consulta
            .OrderByDescending(x => x.Ativo)
            .ThenBy(x => x.Nome)
            .Select(x => new
            {
                x.Id,
                x.Nome,
                x.Email,
                Perfil = x.Perfil.ParaCodigo(),
                x.IgrejaId,
                NomeIgreja = x.Igreja == null ? null : x.Igreja.Nome,
                x.Ativo,
                x.DeveTrocarSenha,
                x.CriadoEm,
                x.AtualizadoEm
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(usuarios);
    }

    private static async Task<IResult> SugerirSenhaAsync(
        UsuarioAtual usuarioAtual,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        if (usuario.Perfil == PerfilUsuario.Equipe)
        {
            return Results.Forbid();
        }

        return Results.Ok(new { senha = GerarSenhaTemporaria() });
    }

    private static async Task<IResult> CriarAsync(
        CriarUsuarioRequisicao requisicao,
        UserManager<Usuario> gerenciadorUsuarios,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        IRelogio relogio,
        CancellationToken cancellationToken)
    {
        var autor = await usuarioAtual.ObterAsync(cancellationToken);
        if (autor is null)
        {
            return Results.Unauthorized();
        }

        if (autor.Perfil == PerfilUsuario.Equipe)
        {
            return Results.Forbid();
        }

        var erros = ValidarDadosBasicos(requisicao.Nome, requisicao.Email, requisicao.Perfil);
        var perfilValido = CodigosDominio.TentarLerPerfil(requisicao.Perfil, out var perfil);

        if (string.IsNullOrWhiteSpace(requisicao.SenhaTemporaria))
        {
            erros["senhaTemporaria"] = ["Informe ou gere uma senha temporária."];
        }

        if (!perfilValido)
        {
            return Results.ValidationProblem(erros);
        }

        if (autor.Perfil == PerfilUsuario.Pastor && perfil == PerfilUsuario.Administrador)
        {
            return Results.Forbid();
        }

        Guid? igrejaId;
        if (perfil == PerfilUsuario.Administrador)
        {
            igrejaId = null;
        }
        else
        {
            igrejaId = autor.Perfil == PerfilUsuario.Pastor ? autor.IgrejaId : requisicao.IgrejaId;
            if (!igrejaId.HasValue)
            {
                erros["igrejaId"] = ["Selecione a igreja do usuário."];
            }
            else if (!await banco.Igrejas.AnyAsync(x => x.Id == igrejaId, cancellationToken))
            {
                erros["igrejaId"] = ["A igreja selecionada não foi encontrada."];
            }
        }

        if (erros.Count > 0)
        {
            return Results.ValidationProblem(erros);
        }

        var agora = relogio.Agora;
        var email = requisicao.Email!.Trim();
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = requisicao.Nome!.Trim(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            LockoutEnabled = true,
            Perfil = perfil,
            IgrejaId = igrejaId,
            Ativo = true,
            DeveTrocarSenha = true,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        var resultado = await gerenciadorUsuarios.CreateAsync(usuario, requisicao.SenhaTemporaria!);
        if (!resultado.Succeeded)
        {
            return Results.ValidationProblem(TraduzirErrosIdentity(resultado));
        }

        banco.RegistrosAuditoriaAdministrativa.Add(RegistroAuditoriaAdministrativa.Criar(
            "USUARIO",
            usuario.Id,
            "CRIADO",
            $"Usuário {usuario.Nome} criado com o perfil {usuario.Perfil.ParaCodigo()}.",
            autor.Id,
            agora));
        await banco.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/usuarios/{usuario.Id}", new { usuario.Id });
    }

    private static async Task<IResult> EditarAsync(
        Guid id,
        EditarUsuarioRequisicao requisicao,
        UserManager<Usuario> gerenciadorUsuarios,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        IRelogio relogio,
        CancellationToken cancellationToken)
    {
        var autor = await usuarioAtual.ObterAsync(cancellationToken);
        if (autor is null)
        {
            return Results.Unauthorized();
        }

        if (autor.Perfil == PerfilUsuario.Equipe)
        {
            return Results.Forbid();
        }

        var alvo = await gerenciadorUsuarios.FindByIdAsync(id.ToString());
        if (alvo is null)
        {
            return Results.NotFound();
        }

        if (!PodeAdministrar(autor, alvo))
        {
            return Results.Forbid();
        }

        var erros = ValidarDadosBasicos(requisicao.Nome, requisicao.Email, requisicao.Perfil);
        var perfilValido = CodigosDominio.TentarLerPerfil(requisicao.Perfil, out var novoPerfil);

        if (!perfilValido)
        {
            return Results.ValidationProblem(erros);
        }

        if (autor.Perfil == PerfilUsuario.Pastor && novoPerfil == PerfilUsuario.Administrador)
        {
            return Results.Forbid();
        }

        var alteraVinculoGlobal = (alvo.Perfil == PerfilUsuario.Administrador) !=
                                  (novoPerfil == PerfilUsuario.Administrador);
        if (alteraVinculoGlobal)
        {
            erros["perfil"] = ["Não é permitido converter uma conta global em conta de igreja, ou o contrário."];
        }

        if (erros.Count > 0)
        {
            return Results.ValidationProblem(erros);
        }

        var agora = relogio.Agora;
        var email = requisicao.Email!.Trim();
        alvo.Nome = requisicao.Nome!.Trim();
        alvo.Email = email;
        alvo.UserName = email;
        alvo.Perfil = novoPerfil;
        alvo.AtualizadoEm = agora;

        var resultado = await gerenciadorUsuarios.UpdateAsync(alvo);
        if (!resultado.Succeeded)
        {
            return Results.ValidationProblem(TraduzirErrosIdentity(resultado));
        }

        banco.RegistrosAuditoriaAdministrativa.Add(RegistroAuditoriaAdministrativa.Criar(
            "USUARIO",
            alvo.Id,
            "EDITADO",
            $"Dados do usuário {alvo.Nome} atualizados.",
            autor.Id,
            agora));
        await banco.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AlterarSituacaoAsync(
        Guid id,
        AlterarSituacaoRequisicao requisicao,
        UserManager<Usuario> gerenciadorUsuarios,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        IRelogio relogio,
        CancellationToken cancellationToken)
    {
        var autor = await usuarioAtual.ObterAsync(cancellationToken);
        if (autor is null)
        {
            return Results.Unauthorized();
        }

        if (autor.Perfil == PerfilUsuario.Equipe)
        {
            return Results.Forbid();
        }

        var alvo = await gerenciadorUsuarios.FindByIdAsync(id.ToString());
        if (alvo is null)
        {
            return Results.NotFound();
        }

        if (!PodeAdministrar(autor, alvo))
        {
            return Results.Forbid();
        }

        if (!requisicao.Ativo && alvo.Id == autor.Id)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["ativo"] = ["Você não pode desativar a própria conta."]
            });
        }

        if (!requisicao.Ativo && alvo.Perfil == PerfilUsuario.Administrador)
        {
            var administradoresAtivos = await banco.Users.CountAsync(
                x => x.Perfil == PerfilUsuario.Administrador && x.Ativo,
                cancellationToken);
            if (administradoresAtivos <= 1)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["ativo"] = ["O último Administrador ativo não pode ser desativado."]
                });
            }
        }

        if (alvo.Ativo == requisicao.Ativo)
        {
            return Results.NoContent();
        }

        var agora = relogio.Agora;
        alvo.Ativo = requisicao.Ativo;
        alvo.AtualizadoEm = agora;
        var resultado = await gerenciadorUsuarios.UpdateAsync(alvo);
        if (!resultado.Succeeded)
        {
            return Results.ValidationProblem(TraduzirErrosIdentity(resultado));
        }

        banco.RegistrosAuditoriaAdministrativa.Add(RegistroAuditoriaAdministrativa.Criar(
            "USUARIO",
            alvo.Id,
            requisicao.Ativo ? "ATIVADO" : "DESATIVADO",
            $"Usuário {alvo.Nome} {(requisicao.Ativo ? "ativado" : "desativado")}.",
            autor.Id,
            agora));
        await banco.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> RedefinirSenhaAsync(
        Guid id,
        RedefinirSenhaRequisicao requisicao,
        UserManager<Usuario> gerenciadorUsuarios,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        IRelogio relogio,
        CancellationToken cancellationToken)
    {
        var autor = await usuarioAtual.ObterAsync(cancellationToken);
        if (autor is null)
        {
            return Results.Unauthorized();
        }

        if (autor.Perfil == PerfilUsuario.Equipe)
        {
            return Results.Forbid();
        }

        var alvo = await gerenciadorUsuarios.FindByIdAsync(id.ToString());
        if (alvo is null)
        {
            return Results.NotFound();
        }

        if (!PodeAdministrar(autor, alvo))
        {
            return Results.Forbid();
        }

        if (string.IsNullOrWhiteSpace(requisicao.SenhaTemporaria))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["senhaTemporaria"] = ["Informe ou gere uma senha temporária."]
            });
        }

        var token = await gerenciadorUsuarios.GeneratePasswordResetTokenAsync(alvo);
        var resultado = await gerenciadorUsuarios.ResetPasswordAsync(
            alvo,
            token,
            requisicao.SenhaTemporaria);
        if (!resultado.Succeeded)
        {
            return Results.ValidationProblem(TraduzirErrosIdentity(resultado));
        }

        var agora = relogio.Agora;
        alvo.DeveTrocarSenha = true;
        alvo.AtualizadoEm = agora;
        await gerenciadorUsuarios.UpdateAsync(alvo);

        banco.RegistrosAuditoriaAdministrativa.Add(RegistroAuditoriaAdministrativa.Criar(
            "USUARIO",
            alvo.Id,
            "SENHA_TEMPORARIA_REDEFINIDA",
            $"Nova senha temporária definida para {alvo.Nome}.",
            autor.Id,
            agora));
        await banco.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static bool PodeAdministrar(Usuario autor, Usuario alvo) =>
        autor.Perfil == PerfilUsuario.Administrador ||
        (autor.Perfil == PerfilUsuario.Pastor &&
         autor.IgrejaId == alvo.IgrejaId &&
         alvo.Perfil != PerfilUsuario.Administrador);

    private static Dictionary<string, string[]> ValidarDadosBasicos(
        string? nome,
        string? email,
        string? perfil)
    {
        var erros = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(nome))
        {
            erros["nome"] = ["Informe o nome do usuário."];
        }
        else if (nome.Trim().Length > 150)
        {
            erros["nome"] = ["O nome deve ter no máximo 150 caracteres."];
        }

        if (string.IsNullOrWhiteSpace(email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email.Trim()))
        {
            erros["email"] = ["Informe um e-mail válido."];
        }

        if (!CodigosDominio.TentarLerPerfil(perfil, out _))
        {
            erros["perfil"] = ["Selecione um perfil válido."];
        }

        return erros;
    }

    private static Dictionary<string, string[]> TraduzirErrosIdentity(IdentityResult resultado)
    {
        var mensagensEmail = new List<string>();
        var mensagensSenha = new List<string>();
        var mensagensGerais = new List<string>();

        foreach (var erro in resultado.Errors)
        {
            if (erro.Code is "DuplicateEmail" or "DuplicateUserName")
            {
                mensagensEmail.Add("Já existe um usuário com este e-mail.");
            }
            else if (erro.Code.StartsWith("Password", StringComparison.Ordinal))
            {
                mensagensSenha.Add("A senha deve ter ao menos 10 caracteres, incluindo maiúscula, minúscula, número e símbolo.");
            }
            else
            {
                mensagensGerais.Add("Não foi possível salvar o usuário. Verifique os dados e tente novamente.");
            }
        }

        var erros = new Dictionary<string, string[]>();
        if (mensagensEmail.Count > 0)
        {
            erros["email"] = mensagensEmail.Distinct().ToArray();
        }

        if (mensagensSenha.Count > 0)
        {
            erros["senhaTemporaria"] = mensagensSenha.Distinct().ToArray();
        }

        if (mensagensGerais.Count > 0)
        {
            erros["usuario"] = mensagensGerais.Distinct().ToArray();
        }

        return erros;
    }

    private static string GerarSenhaTemporaria()
    {
        var primeira = PalavrasSenha[RandomNumberGenerator.GetInt32(PalavrasSenha.Length)];
        var segunda = PalavrasSenha[RandomNumberGenerator.GetInt32(PalavrasSenha.Length)];
        var numero = RandomNumberGenerator.GetInt32(100, 1_000);
        var sufixo = new char[6];
        for (var indice = 0; indice < sufixo.Length; indice++)
        {
            sufixo[indice] = CaracteresSenha[RandomNumberGenerator.GetInt32(CaracteresSenha.Length)];
        }

        return $"{primeira}-{segunda}-{numero}!{new string(sufixo)}";
    }
}
