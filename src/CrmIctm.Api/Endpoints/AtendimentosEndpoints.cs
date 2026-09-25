using CrmIctm.Api.Contratos;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Infraestrutura;
using CrmIctm.Api.Servicos;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;

namespace CrmIctm.Api.Endpoints;

public static class AtendimentosEndpoints
{
    public static IEndpointRouteBuilder MapearAtendimentos(this IEndpointRouteBuilder endpoints)
    {
        var grupo = endpoints.MapGroup("/api/atendimentos")
            .WithTags("Atendimentos")
            .RequireAuthorization();

        grupo.MapGet("", ListarAsync);
        grupo.MapGet("/{id:guid}", ObterDetalheAsync);
        grupo.MapPost("/{id:guid}/observacoes", AdicionarObservacaoAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        grupo.MapPut("/{id:guid}/etapa", AlterarEtapaAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        grupo.MapPut("/{id:guid}/responsavel", AtribuirResponsavelAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        grupo.MapPut("/{id:guid}/dados-complementares", AtualizarDadosComplementaresAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());

        return endpoints;
    }

    private static async Task<IResult> ListarAsync(
        Guid? igrejaId,
        string? termo,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        if (!TentarObterIgrejaAutorizada(usuario, igrejaId, out var igrejaAutorizada, out var erro))
        {
            return erro!;
        }

        var consulta = banco.Atendimentos
            .AsNoTracking()
            .Where(x => x.IgrejaId == igrejaAutorizada);

        var termoPesquisa = termo?.Trim();
        if (!string.IsNullOrWhiteSpace(termoPesquisa))
        {
            var digitos = new string(termoPesquisa.Where(char.IsAsciiDigit).ToArray());
            consulta = string.Equals(
                banco.Database.ProviderName,
                "Microsoft.EntityFrameworkCore.InMemory",
                StringComparison.Ordinal)
                ? consulta.Where(x =>
                    x.NomeVisitante.Contains(termoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                    (digitos.Length > 0 && x.Whatsapp.Contains(digitos)))
                : consulta.Where(x =>
                    EF.Functions.ILike(x.NomeVisitante, $"%{termoPesquisa}%") ||
                    (digitos.Length > 0 && x.Whatsapp.Contains(digitos)));
        }

        var registros = await consulta
            .OrderByDescending(x => x.UltimaAtividadeEm)
            .Select(x => new
            {
                x.Id,
                x.NomeVisitante,
                x.Whatsapp,
                x.QuerConhecerIgreja,
                x.QuerConversaOracao,
                x.Etapa,
                x.ResponsavelId,
                NomeResponsavel = x.Responsavel == null ? null : x.Responsavel.Nome,
                x.CriadoEm,
                x.UltimaAtividadeEm,
                x.Resultado,
                x.ConcluidoEm
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(registros.Select(x => new
        {
            x.Id,
            x.NomeVisitante,
            x.Whatsapp,
            x.QuerConhecerIgreja,
            x.QuerConversaOracao,
            Etapa = x.Etapa.ParaCodigo(),
            x.ResponsavelId,
            x.NomeResponsavel,
            x.CriadoEm,
            x.UltimaAtividadeEm,
            Resultado = x.Resultado?.ParaCodigo(),
            x.ConcluidoEm
        }));
    }

    private static async Task<IResult> ObterDetalheAsync(
        Guid id,
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

        if (!TentarObterIgrejaAutorizada(usuario, igrejaId, out var igrejaAutorizada, out var erro))
        {
            return erro!;
        }

        var atendimento = await banco.Atendimentos
            .AsNoTracking()
            .Include(x => x.Igreja)
            .Include(x => x.Responsavel)
            .SingleOrDefaultAsync(
                x => x.Id == id && x.IgrejaId == igrejaAutorizada,
                cancellationToken);

        if (atendimento is null)
        {
            return Results.NotFound();
        }

        var observacoes = await banco.ObservacoesAtendimento
            .AsNoTracking()
            .Where(x => x.AtendimentoId == id)
            .OrderByDescending(x => x.CriadaEm)
            .Select(x => new { x.Id, x.Texto, x.CriadaEm, NomeAutor = x.Autor.Nome })
            .ToListAsync(cancellationToken);

        var historico = await banco.HistoricosAtendimento
            .AsNoTracking()
            .Where(x => x.AtendimentoId == id)
            .OrderByDescending(x => x.CriadoEm)
            .Select(x => new { x.Id, x.Tipo, x.Descricao, x.CriadoEm, NomeAutor = x.Autor.Nome })
            .ToListAsync(cancellationToken);

        return Results.Ok(new
        {
            atendimento.Id,
            atendimento.IgrejaId,
            NomeIgreja = atendimento.Igreja.Nome,
            atendimento.NomeVisitante,
            atendimento.Whatsapp,
            atendimento.QuerConhecerIgreja,
            atendimento.QuerConversaOracao,
            Etapa = atendimento.Etapa.ParaCodigo(),
            atendimento.ResponsavelId,
            NomeResponsavel = atendimento.Responsavel?.Nome,
            atendimento.LogradouroVisitante,
            atendimento.NumeroResidencia,
            atendimento.BairroVisitante,
            atendimento.ComplementoEndereco,
            atendimento.DataNascimento,
            SituacaoCongregacional = atendimento.SituacaoCongregacional.ParaCodigo(),
            atendimento.NomeIgrejaCongrega,
            atendimento.CriadoEm,
            atendimento.UltimaAtividadeEm,
            Resultado = atendimento.Resultado?.ParaCodigo(),
            atendimento.ConcluidoEm,
            Observacoes = observacoes,
            Historico = historico
        });
    }

    private static async Task<IResult> AdicionarObservacaoAsync(
        Guid id,
        Guid? igrejaId,
        AdicionarObservacaoRequisicao requisicao,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        IRelogio relogio,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        var texto = requisicao.Texto?.Trim();
        if (string.IsNullOrWhiteSpace(texto) || texto.Length > 2_000)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["texto"] = ["Informe uma observação de até 2.000 caracteres."]
            });
        }

        if (!TentarObterIgrejaAutorizada(usuario, igrejaId, out var igrejaAutorizada, out var erro))
        {
            return erro!;
        }

        var atendimento = await banco.Atendimentos
            .SingleOrDefaultAsync(
                x => x.Id == id && x.IgrejaId == igrejaAutorizada,
                cancellationToken);
        if (atendimento is null)
        {
            return Results.NotFound();
        }

        var agora = relogio.Agora;
        atendimento.RegistrarAtividade(agora);
        banco.ObservacoesAtendimento.Add(new ObservacaoAtendimento
        {
            Id = Guid.NewGuid(),
            AtendimentoId = atendimento.Id,
            AutorId = usuario.Id,
            Texto = texto,
            CriadaEm = agora
        });
        await banco.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AlterarEtapaAsync(
        Guid id,
        Guid? igrejaId,
        AlterarEtapaRequisicao requisicao,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        IRelogio relogio,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        if (!CodigosDominio.TentarLerEtapa(requisicao.Etapa, out var novaEtapa))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["etapa"] = ["Selecione uma etapa válida."]
            });
        }

        if (!TentarObterIgrejaAutorizada(usuario, igrejaId, out var igrejaAutorizada, out var erro))
        {
            return erro!;
        }

        var atendimento = await banco.Atendimentos
            .SingleOrDefaultAsync(
                x => x.Id == id && x.IgrejaId == igrejaAutorizada,
                cancellationToken);
        if (atendimento is null)
        {
            return Results.NotFound();
        }

        if (Math.Abs((int)novaEtapa - (int)atendimento.Etapa) != 1)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["etapa"] = ["Mova o atendimento somente para a etapa imediatamente anterior ou posterior."]
            });
        }

        ResultadoAtendimento? novoResultado = null;
        if (novaEtapa == EtapaAtendimento.Concluido)
        {
            if (!CodigosDominio.TentarLerResultado(requisicao.Resultado, out var resultado))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["resultado"] = ["Selecione um resultado para concluir o atendimento."]
                });
            }

            novoResultado = resultado;
        }

        var etapaAnterior = atendimento.Etapa;
        var resultadoAnterior = atendimento.Resultado;
        var agora = relogio.Agora;
        atendimento.AlterarEtapa(novaEtapa, novoResultado, agora);

        var reabertura = etapaAnterior == EtapaAtendimento.Concluido &&
                         novaEtapa != EtapaAtendimento.Concluido;
        var descricao = reabertura
            ? $"Atendimento reaberto para {novaEtapa.ParaCodigo()}. Resultado anterior: {resultadoAnterior?.ParaCodigo()}."
            : $"Etapa alterada de {etapaAnterior.ParaCodigo()} para {novaEtapa.ParaCodigo()}.";

        banco.HistoricosAtendimento.Add(new HistoricoAtendimento
        {
            Id = Guid.NewGuid(),
            AtendimentoId = atendimento.Id,
            AutorId = usuario.Id,
            Tipo = reabertura ? "REABERTURA" : novaEtapa == EtapaAtendimento.Concluido ? "CONCLUSAO" : "MUDANCA_ETAPA",
            Descricao = descricao,
            CriadoEm = agora
        });
        await banco.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AtribuirResponsavelAsync(
        Guid id,
        Guid? igrejaId,
        AtribuirResponsavelRequisicao requisicao,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        IRelogio relogio,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        if (!TentarObterIgrejaAutorizada(usuario, igrejaId, out var igrejaAutorizada, out var erro))
        {
            return erro!;
        }

        var atendimento = await banco.Atendimentos
            .SingleOrDefaultAsync(
                x => x.Id == id && x.IgrejaId == igrejaAutorizada,
                cancellationToken);
        if (atendimento is null)
        {
            return Results.NotFound();
        }

        Usuario? responsavel = null;
        if (requisicao.ResponsavelId.HasValue)
        {
            responsavel = await banco.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == requisicao.ResponsavelId &&
                         x.Ativo &&
                         x.IgrejaId == atendimento.IgrejaId,
                    cancellationToken);
            if (responsavel is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["responsavelId"] = ["Selecione um usuário ativo desta igreja."]
                });
            }
        }

        var agora = relogio.Agora;
        atendimento.AtribuirResponsavel(responsavel?.Id, agora);
        banco.HistoricosAtendimento.Add(new HistoricoAtendimento
        {
            Id = Guid.NewGuid(),
            AtendimentoId = atendimento.Id,
            AutorId = usuario.Id,
            Tipo = "ATRIBUICAO",
            Descricao = responsavel is null
                ? "Responsável removido do atendimento."
                : $"Atendimento atribuído a {responsavel.Nome}.",
            CriadoEm = agora
        });
        await banco.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AtualizarDadosComplementaresAsync(
        Guid id,
        Guid? igrejaId,
        AtualizarDadosComplementaresRequisicao requisicao,
        BancoContexto banco,
        UsuarioAtual usuarioAtual,
        IRelogio relogio,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        var erros = new Dictionary<string, string[]>();
        ValidarTextoOpcional(erros, "logradouro", requisicao.Logradouro, 150);
        ValidarTextoOpcional(erros, "numeroResidencia", requisicao.NumeroResidencia, 20);
        ValidarTextoOpcional(erros, "bairro", requisicao.Bairro, 100);
        ValidarTextoOpcional(erros, "complemento", requisicao.Complemento, 100);
        ValidarTextoOpcional(erros, "nomeIgrejaCongrega", requisicao.NomeIgrejaCongrega, 150);

        if (!CodigosDominio.TentarLerSituacaoCongregacional(
                requisicao.SituacaoCongregacional,
                out var situacaoCongregacional))
        {
            erros["situacaoCongregacional"] = ["Selecione uma situação congregacional válida."];
        }

        var agora = relogio.Agora;
        if (requisicao.DataNascimento.HasValue &&
            requisicao.DataNascimento.Value > DateOnly.FromDateTime(agora.LocalDateTime))
        {
            erros["dataNascimento"] = ["A data de nascimento não pode estar no futuro."];
        }

        if (erros.Count > 0)
        {
            return Results.ValidationProblem(erros);
        }

        if (!TentarObterIgrejaAutorizada(usuario, igrejaId, out var igrejaAutorizada, out var erro))
        {
            return erro!;
        }

        var atendimento = await banco.Atendimentos
            .SingleOrDefaultAsync(
                x => x.Id == id && x.IgrejaId == igrejaAutorizada,
                cancellationToken);
        if (atendimento is null)
        {
            return Results.NotFound();
        }

        atendimento.AtualizarDadosComplementares(
            requisicao.Logradouro,
            requisicao.NumeroResidencia,
            requisicao.Bairro,
            requisicao.Complemento,
            requisicao.DataNascimento,
            situacaoCongregacional,
            requisicao.NomeIgrejaCongrega,
            agora);
        banco.HistoricosAtendimento.Add(new HistoricoAtendimento
        {
            Id = Guid.NewGuid(),
            AtendimentoId = atendimento.Id,
            AutorId = usuario.Id,
            Tipo = "DADOS_COMPLEMENTARES",
            Descricao = "Dados complementares do visitante atualizados.",
            CriadoEm = agora
        });
        await banco.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static bool TentarObterIgrejaAutorizada(
        Usuario usuario,
        Guid? igrejaId,
        out Guid igrejaAutorizada,
        out IResult? erro)
    {
        igrejaAutorizada = Guid.Empty;
        erro = null;

        if (usuario.Perfil == PerfilUsuario.Administrador)
        {
            if (!igrejaId.HasValue)
            {
                erro = Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["igrejaId"] = ["Selecione uma igreja."]
                });
                return false;
            }

            igrejaAutorizada = igrejaId.Value;
            return true;
        }

        if (!usuario.IgrejaId.HasValue ||
            (igrejaId.HasValue && igrejaId.Value != usuario.IgrejaId.Value))
        {
            erro = Results.Forbid();
            return false;
        }

        igrejaAutorizada = usuario.IgrejaId.Value;
        return true;
    }

    private static void ValidarTextoOpcional(
        Dictionary<string, string[]> erros,
        string campo,
        string? valor,
        int tamanhoMaximo)
    {
        if (!string.IsNullOrWhiteSpace(valor) && valor.Trim().Length > tamanhoMaximo)
        {
            erros[campo] = [$"O campo deve ter no máximo {tamanhoMaximo} caracteres."];
        }
    }
}
