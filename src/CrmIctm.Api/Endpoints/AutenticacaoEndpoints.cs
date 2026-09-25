using CrmIctm.Api.Contratos;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Infraestrutura;
using CrmIctm.Api.Servicos;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CrmIctm.Api.Endpoints;

public static class AutenticacaoEndpoints
{
    public static IEndpointRouteBuilder MapearAutenticacao(this IEndpointRouteBuilder endpoints)
    {
        var grupo = endpoints.MapGroup("/api/autenticacao")
            .WithTags("Autenticação");

        grupo.MapPost("/entrar", EntrarAsync)
            .AllowAnonymous()
            .WithMetadata(new RequireAntiforgeryTokenAttribute())
            .RequireRateLimiting("autenticacao");

        grupo.MapPost("/sair", async (SignInManager<Usuario> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.NoContent();
        }).RequireAuthorization().WithMetadata(new RequireAntiforgeryTokenAttribute());

        grupo.MapPost("/alterar-senha", AlterarSenhaAsync)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute());

        grupo.MapGet("/sessao", async (UsuarioAtual usuarioAtual, CancellationToken cancellationToken) =>
        {
            var usuario = await usuarioAtual.ObterAsync(cancellationToken, permitirTrocaSenhaPendente: true);
            return usuario is null
                ? Results.Unauthorized()
                : Results.Ok(new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email,
                    Perfil = usuario.Perfil.ParaCodigo(),
                    usuario.IgrejaId,
                    usuario.DeveTrocarSenha
                });
        }).RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> EntrarAsync(
        EntrarRequisicao requisicao,
        BancoContexto banco,
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager)
    {
        if (string.IsNullOrWhiteSpace(requisicao.Email) || string.IsNullOrWhiteSpace(requisicao.Senha))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["credenciais"] = ["Informe e-mail e senha."]
            });
        }

        var usuario = await userManager.FindByEmailAsync(requisicao.Email.Trim());
        if (usuario is null || !usuario.Ativo)
        {
            return CredenciaisInvalidas();
        }

        if (usuario.IgrejaId.HasValue &&
            !await banco.Igrejas.AnyAsync(x => x.Id == usuario.IgrejaId && x.Ativa))
        {
            return CredenciaisInvalidas();
        }

        var resultado = await signInManager.PasswordSignInAsync(
            usuario,
            requisicao.Senha,
            isPersistent: false,
            lockoutOnFailure: true);

        return resultado.Succeeded ? Results.NoContent() : CredenciaisInvalidas();
    }

    private static async Task<IResult> AlterarSenhaAsync(
        AlterarSenhaRequisicao requisicao,
        UsuarioAtual usuarioAtual,
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        BancoContexto banco,
        IRelogio relogio,
        CancellationToken cancellationToken)
    {
        var usuarioAtualBanco = await usuarioAtual.ObterAsync(
            cancellationToken,
            permitirTrocaSenhaPendente: true);
        if (usuarioAtualBanco is null)
        {
            return Results.Unauthorized();
        }

        var erros = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(requisicao.SenhaAtual))
        {
            erros["senhaAtual"] = ["Informe a senha temporária atual."];
        }

        if (string.IsNullOrWhiteSpace(requisicao.NovaSenha))
        {
            erros["novaSenha"] = ["Informe a nova senha."];
        }
        else if (!string.Equals(requisicao.NovaSenha, requisicao.ConfirmacaoSenha, StringComparison.Ordinal))
        {
            erros["confirmacaoSenha"] = ["A confirmação não corresponde à nova senha."];
        }

        if (erros.Count > 0)
        {
            return Results.ValidationProblem(erros);
        }

        var usuario = await userManager.FindByIdAsync(usuarioAtualBanco.Id.ToString());
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        var resultado = await userManager.ChangePasswordAsync(
            usuario,
            requisicao.SenhaAtual!,
            requisicao.NovaSenha!);
        if (!resultado.Succeeded)
        {
            var senhaAtualIncorreta = resultado.Errors.Any(x => x.Code == "PasswordMismatch");
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [senhaAtualIncorreta ? "senhaAtual" : "novaSenha"] =
                [senhaAtualIncorreta
                    ? "A senha atual está incorreta."
                    : "A nova senha deve ter ao menos 10 caracteres, incluindo maiúscula, minúscula, número e símbolo."]
            });
        }

        var agora = relogio.Agora;
        usuario.DeveTrocarSenha = false;
        usuario.AtualizadoEm = agora;
        await userManager.UpdateAsync(usuario);

        banco.RegistrosAuditoriaAdministrativa.Add(RegistroAuditoriaAdministrativa.Criar(
            "USUARIO",
            usuario.Id,
            "SENHA_ALTERADA",
            $"Senha alterada pelo usuário {usuario.Nome}.",
            usuario.Id,
            agora));
        await banco.SaveChangesAsync(cancellationToken);
        await signInManager.RefreshSignInAsync(usuario);

        return Results.NoContent();
    }

    private static IResult CredenciaisInvalidas() => Results.Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Não foi possível entrar",
        detail: "E-mail ou senha inválidos.");
}
