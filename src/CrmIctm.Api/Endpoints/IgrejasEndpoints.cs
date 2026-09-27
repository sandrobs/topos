using CrmIctm.Api.Contratos;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Infraestrutura;
using CrmIctm.Api.Servicos;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace CrmIctm.Api.Endpoints;

public static class IgrejasEndpoints
{
    public static IEndpointRouteBuilder MapearIgrejas(this IEndpointRouteBuilder endpoints)
    {
        var grupo = endpoints.MapGroup("/api/igrejas")
            .WithTags("Igrejas")
            .RequireAuthorization();

        grupo.MapGet("", ListarAsync);
        grupo.MapGet("/{id:guid}/qrcode", ObterQrCodeAsync);
        grupo.MapGet("/{id:guid}/convite.pdf", ObterConvitePdfAsync)
            .RequireRateLimiting("convite-pdf");
        grupo.MapPost("", CriarAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        grupo.MapPut("/{id:guid}", EditarAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        grupo.MapPut("/{id:guid}/situacao", AlterarSituacaoAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());

        return endpoints;
    }

    private static async Task<IResult> ListarAsync(
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        var consulta = banco.Igrejas.AsNoTracking();
        if (usuario.Perfil != PerfilUsuario.Administrador)
        {
            consulta = consulta.Where(x => x.Id == usuario.IgrejaId);
        }

        var igrejas = await consulta
            .OrderBy(x => x.Nome)
            .Select(x => new
            {
                x.Id,
                x.Nome,
                x.Logradouro,
                x.Numero,
                x.Bairro,
                x.Complemento,
                x.Cep,
                x.Cidade,
                x.Estado,
                x.Ativa,
                x.IdentificadorPublico,
                x.CriadaEm,
                x.AtualizadaEm
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(igrejas);
    }

    private static async Task<IResult> CriarAsync(
        SalvarIgrejaRequisicao requisicao,
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

        if (autor.Perfil != PerfilUsuario.Administrador)
        {
            return Results.Forbid();
        }

        var erros = Validar(requisicao);
        if (erros.Count > 0)
        {
            return Results.ValidationProblem(erros);
        }

        var agora = relogio.Agora;
        var igreja = Igreja.Criar(
            requisicao.Nome!,
            requisicao.Logradouro!,
            requisicao.Numero!,
            requisicao.Bairro!,
            requisicao.Complemento,
            requisicao.Cep!,
            requisicao.Cidade!,
            requisicao.Estado!,
            agora);

        banco.Igrejas.Add(igreja);
        banco.RegistrosAuditoriaAdministrativa.Add(RegistroAuditoriaAdministrativa.Criar(
            "IGREJA",
            igreja.Id,
            "CRIADA",
            $"Igreja {igreja.Nome} criada.",
            autor.Id,
            agora));
        await banco.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/igrejas/{igreja.Id}", new { igreja.Id });
    }

    private static async Task<IResult> EditarAsync(
        Guid id,
        SalvarIgrejaRequisicao requisicao,
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

        if (autor.Perfil != PerfilUsuario.Administrador)
        {
            return Results.Forbid();
        }

        var erros = Validar(requisicao);
        if (erros.Count > 0)
        {
            return Results.ValidationProblem(erros);
        }

        var igreja = await banco.Igrejas.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (igreja is null)
        {
            return Results.NotFound();
        }

        var agora = relogio.Agora;
        igreja.AtualizarDados(
            requisicao.Nome!,
            requisicao.Logradouro!,
            requisicao.Numero!,
            requisicao.Bairro!,
            requisicao.Complemento,
            requisicao.Cep!,
            requisicao.Cidade!,
            requisicao.Estado!,
            agora);
        banco.RegistrosAuditoriaAdministrativa.Add(RegistroAuditoriaAdministrativa.Criar(
            "IGREJA",
            igreja.Id,
            "EDITADA",
            $"Dados da igreja {igreja.Nome} atualizados.",
            autor.Id,
            agora));
        await banco.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AlterarSituacaoAsync(
        Guid id,
        AlterarSituacaoRequisicao requisicao,
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

        if (autor.Perfil != PerfilUsuario.Administrador)
        {
            return Results.Forbid();
        }

        var igreja = await banco.Igrejas.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (igreja is null)
        {
            return Results.NotFound();
        }

        if (igreja.Ativa == requisicao.Ativo)
        {
            return Results.NoContent();
        }

        var agora = relogio.Agora;
        igreja.AlterarSituacao(requisicao.Ativo, agora);
        banco.RegistrosAuditoriaAdministrativa.Add(RegistroAuditoriaAdministrativa.Criar(
            "IGREJA",
            igreja.Id,
            requisicao.Ativo ? "ATIVADA" : "DESATIVADA",
            $"Igreja {igreja.Nome} {(requisicao.Ativo ? "ativada" : "desativada")}.",
            autor.Id,
            agora));
        await banco.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ObterQrCodeAsync(
        Guid id,
        HttpRequest requisicao,
        HttpResponse resposta,
        IConfiguration configuracao,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        var consulta = banco.Igrejas.AsNoTracking().Where(x => x.Id == id);
        if (usuario.Perfil != PerfilUsuario.Administrador)
        {
            consulta = consulta.Where(x => x.Id == usuario.IgrejaId);
        }

        var igreja = await consulta.SingleOrDefaultAsync(cancellationToken);
        if (igreja is null)
        {
            return Results.NotFound();
        }

        var url = ObterEnderecoFormulario(igreja, requisicao, configuracao);
        using var dadosQrCode = QRCodeGenerator.GenerateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var svg = new SvgQRCode(dadosQrCode).GetGraphic(8);
        resposta.Headers.ContentDisposition = $"inline; filename=qr-code-{igreja.Id:N}.svg";
        resposta.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        resposta.Headers.Pragma = "no-cache";
        return Results.Content(svg, "image/svg+xml", System.Text.Encoding.UTF8);
    }

    private static async Task<IResult> ObterConvitePdfAsync(
        Guid id,
        HttpRequest requisicao,
        HttpResponse resposta,
        IConfiguration configuracao,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        GeradorConviteVisitantesPdf gerador,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null) return Results.Unauthorized();

        var consulta = banco.Igrejas.AsNoTracking().Where(x => x.Id == id && x.Ativa);
        if (usuario.Perfil != PerfilUsuario.Administrador)
        {
            consulta = consulta.Where(x => x.Id == usuario.IgrejaId);
        }
        var igreja = await consulta.SingleOrDefaultAsync(cancellationToken);
        if (igreja is null) return Results.NotFound();

        var pdf = gerador.Gerar(igreja, ObterEnderecoFormulario(igreja, requisicao, configuracao));
        resposta.Headers.ContentDisposition = $"inline; filename=convite-visitantes-{igreja.Id:N}.pdf";
        resposta.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        resposta.Headers.Pragma = "no-cache";
        resposta.Headers.XContentTypeOptions = "nosniff";
        return Results.File(pdf, "application/pdf");
    }

    private static string ObterEnderecoFormulario(Igreja igreja, HttpRequest requisicao, IConfiguration configuracao)
    {
        var origem = configuracao["Aplicacao:UrlPublica"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(origem)) origem = $"{requisicao.Scheme}://{requisicao.Host}";
        return $"{origem}/visita/{igreja.IdentificadorPublico}";
    }

    private static Dictionary<string, string[]> Validar(SalvarIgrejaRequisicao requisicao)
    {
        var erros = new Dictionary<string, string[]>();
        ValidarTexto(erros, "nome", requisicao.Nome, "Informe o nome da igreja.", 150);
        ValidarTexto(erros, "logradouro", requisicao.Logradouro, "Informe a rua ou avenida.", 150);
        ValidarTexto(erros, "numero", requisicao.Numero, "Informe o número ou S/N.", 20);
        ValidarTexto(erros, "bairro", requisicao.Bairro, "Informe o bairro.", 100);
        ValidarTexto(erros, "cidade", requisicao.Cidade, "Informe a cidade.", 100);

        if (!string.IsNullOrWhiteSpace(requisicao.Complemento) && requisicao.Complemento.Trim().Length > 100)
        {
            erros["complemento"] = ["O complemento deve ter no máximo 100 caracteres."];
        }

        var cep = new string((requisicao.Cep ?? string.Empty).Where(char.IsDigit).ToArray());
        if (cep.Length != 8)
        {
            erros["cep"] = ["Informe um CEP válido com oito dígitos."];
        }

        if (!UnidadesFederativas.EhValida(requisicao.Estado))
        {
            erros["estado"] = ["Selecione uma UF brasileira válida."];
        }

        return erros;
    }

    private static void ValidarTexto(
        Dictionary<string, string[]> erros,
        string campo,
        string? valor,
        string mensagemObrigatorio,
        int tamanhoMaximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            erros[campo] = [mensagemObrigatorio];
        }
        else if (valor.Trim().Length > tamanhoMaximo)
        {
            erros[campo] = [$"O campo deve ter no máximo {tamanhoMaximo} caracteres."];
        }
    }
}
