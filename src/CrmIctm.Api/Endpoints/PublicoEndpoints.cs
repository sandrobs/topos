using CrmIctm.Api.Configuracao;
using CrmIctm.Api.Contratos;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Infraestrutura;
using CrmIctm.Api.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CrmIctm.Api.Endpoints;

public static class PublicoEndpoints
{
    public static IEndpointRouteBuilder MapearPublico(this IEndpointRouteBuilder endpoints)
    {
        var grupo = endpoints.MapGroup("/api/publico/igrejas")
            .WithTags("Formulário público")
            .AllowAnonymous();

        grupo.MapGet("/{identificadorPublico}", ObterIgrejaAsync);
        grupo.MapPost("/{identificadorPublico}/atendimentos", CriarAtendimentoAsync)
            .RequireRateLimiting("formulario-publico");

        return endpoints;
    }

    private static async Task<IResult> ObterIgrejaAsync(
        string identificadorPublico,
        BancoContexto banco,
        IOptions<AvisoPrivacidadeOpcoes> avisoPrivacidade,
        CancellationToken cancellationToken)
    {
        var igreja = await banco.Igrejas
            .AsNoTracking()
            .Where(x => x.IdentificadorPublico == identificadorPublico && x.Ativa)
            .Select(x => new
            {
                x.Nome,
                x.Logradouro,
                x.Numero,
                x.Bairro,
                x.Complemento,
                x.Cep,
                x.Cidade,
                x.Estado
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (igreja is null)
        {
            return PaginaIndisponivel();
        }

        var aviso = avisoPrivacidade.Value;
        return Results.Ok(new
        {
            igreja.Nome,
            igreja.Logradouro,
            igreja.Numero,
            igreja.Bairro,
            igreja.Complemento,
            igreja.Cep,
            igreja.Cidade,
            igreja.Estado,
            AvisoPrivacidade = aviso.EstaConfigurado
                ? new { aviso.Versao, aviso.Texto }
                : null
        });
    }

    private static async Task<IResult> CriarAtendimentoAsync(
        string identificadorPublico,
        NovoAtendimentoPublicoRequisicao requisicao,
        BancoContexto banco,
        IOptions<AvisoPrivacidadeOpcoes> avisoPrivacidade,
        IRelogio relogio,
        CancellationToken cancellationToken)
    {
        var igreja = await banco.Igrejas
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.IdentificadorPublico == identificadorPublico && x.Ativa,
                cancellationToken);

        if (igreja is null)
        {
            return PaginaIndisponivel();
        }

        var aviso = avisoPrivacidade.Value;
        if (!aviso.EstaConfigurado)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Cadastro temporariamente indisponível",
                detail: "Tente novamente mais tarde.");
        }

        var erros = Validar(requisicao, aviso);
        if (erros.Count > 0)
        {
            return Results.ValidationProblem(erros);
        }

        _ = NormalizadorWhatsapp.TentarNormalizar(requisicao.Whatsapp, out var whatsapp);
        var atendimento = Atendimento.Criar(
            igreja.Id,
            requisicao.Nome!,
            whatsapp,
            requisicao.QuerConhecerIgreja,
            requisicao.QuerConversaOracao,
            aviso.Versao,
            relogio.Agora);

        banco.Atendimentos.Add(atendimento);
        await banco.SaveChangesAsync(cancellationToken);

        return Results.Ok(new
        {
            mensagem = "Recebemos seus dados. A equipe da igreja entrará em contato pelo WhatsApp."
        });
    }

    private static Dictionary<string, string[]> Validar(
        NovoAtendimentoPublicoRequisicao requisicao,
        AvisoPrivacidadeOpcoes aviso)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(requisicao.Nome))
        {
            erros["nome"] = ["Informe seu nome."];
        }
        else if (requisicao.Nome.Trim().Length > 150)
        {
            erros["nome"] = ["O nome deve ter no máximo 150 caracteres."];
        }

        if (!NormalizadorWhatsapp.TentarNormalizar(requisicao.Whatsapp, out _))
        {
            erros["whatsapp"] = ["Informe um WhatsApp brasileiro válido, com DDD."];
        }

        if (!requisicao.QuerConhecerIgreja && !requisicao.QuerConversaOracao)
        {
            erros["interesses"] = ["Selecione ao menos um interesse."];
        }

        if (!requisicao.AvisoPrivacidadeReconhecido ||
            !string.Equals(requisicao.AvisoPrivacidadeVersao, aviso.Versao, StringComparison.Ordinal))
        {
            erros["avisoPrivacidade"] = ["Leia e reconheça o aviso de privacidade atual."];
        }

        return erros;
    }

    private static IResult PaginaIndisponivel() => Results.Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Página indisponível",
        detail: "Confira o endereço ou procure a equipe da igreja.");
}
