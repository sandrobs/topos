using CrmIctm.Api.Configuracao;
using CrmIctm.Api.Contratos;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Infraestrutura;
using CrmIctm.Api.Servicos;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QRCoder;

namespace CrmIctm.Api.Endpoints;

public static class MembrosEndpoints
{
    public static IEndpointRouteBuilder MapearMembros(this IEndpointRouteBuilder endpoints)
    {
        var publico = endpoints.MapGroup("/api/publico/membros")
            .WithTags("Cadastro público de membros").AllowAnonymous();
        publico.MapGet("/{identificador}", ObterFormularioAsync);
        publico.MapPost("/{identificador}/solicitacoes", EnviarSolicitacaoAsync)
            .RequireRateLimiting("formulario-publico");

        var interno = endpoints.MapGroup("/api/membros")
            .WithTags("Membros").RequireAuthorization();
        interno.MapGet("/resumo", ObterResumoAsync);
        interno.MapGet("", ListarAsync);
        interno.MapGet("/solicitacoes", ListarSolicitacoesAsync);
        interno.MapGet("/solicitacoes/{id:guid}", ObterSolicitacaoAsync);
        interno.MapGet("/qrcode", ObterQrCodeAsync);
        interno.MapGet("/{id:guid}", ObterMembroAsync);
        interno.MapPost("", CriarAsync).WithMetadata(new RequireAntiforgeryTokenAttribute());
        interno.MapPut("/{id:guid}", EditarAsync).WithMetadata(new RequireAntiforgeryTokenAttribute());
        interno.MapPut("/{id:guid}/situacao", AlterarSituacaoAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        interno.MapPut("/solicitacoes/{id:guid}", CorrigirSolicitacaoAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        interno.MapPost("/solicitacoes/{id:guid}/aprovar", AprovarAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        interno.MapPost("/solicitacoes/{id:guid}/recusar", RecusarAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
        return endpoints;
    }

    private static async Task<IResult> ObterFormularioAsync(
        string identificador, BancoContexto banco,
        IOptions<AvisoPrivacidadeMembrosOpcoes> opcoes, CancellationToken cancelamento)
    {
        var igreja = await banco.Igrejas.AsNoTracking()
            .Where(x => x.IdentificadorPublicoMembros == identificador && x.Ativa)
            .Select(x => new { x.Nome, x.Logradouro, x.Numero, x.Bairro,
                x.Complemento, x.Cep, x.Cidade, x.Estado })
            .SingleOrDefaultAsync(cancelamento);
        if (igreja is null) return Results.NotFound();

        var aviso = opcoes.Value;
        return Results.Ok(new
        {
            igreja.Nome, igreja.Logradouro, igreja.Numero, igreja.Bairro,
            igreja.Complemento, igreja.Cep, igreja.Cidade, igreja.Estado,
            AvisoPrivacidade = aviso.EstaConfigurado
                ? new { aviso.Versao, aviso.Texto } : null
        });
    }

    private static async Task<IResult> EnviarSolicitacaoAsync(
        string identificador, EnviarSolicitacaoMembroRequisicao requisicao,
        BancoContexto banco, IOptions<AvisoPrivacidadeMembrosOpcoes> opcoes,
        IRelogio relogio, CancellationToken cancelamento)
    {
        var igreja = await banco.Igrejas.AsNoTracking().SingleOrDefaultAsync(
            x => x.IdentificadorPublicoMembros == identificador && x.Ativa, cancelamento);
        if (igreja is null) return Results.NotFound();

        var aviso = opcoes.Value;
        if (!aviso.EstaConfigurado)
            return Results.Problem(statusCode: 503, title: "Cadastro temporariamente indisponível");

        var erros = ValidacaoDadosMembro.Validar(
            requisicao.Dados, true, DateOnly.FromDateTime(relogio.Agora.UtcDateTime));
        if (!requisicao.DeclaracaoMaioridade)
            erros["declaracaoMaioridade"] = ["Confirme que você é maior de idade."];
        if (!requisicao.AvisoPrivacidadeReconhecido ||
            !string.Equals(requisicao.AvisoPrivacidadeVersao, aviso.Versao, StringComparison.Ordinal))
            erros["avisoPrivacidade"] = ["Leia e reconheça o aviso de privacidade atual."];
        if (erros.Count > 0) return Results.ValidationProblem(erros);

        banco.SolicitacoesMembros.Add(SolicitacaoMembro.Criar(
            igreja.Id, ValidacaoDadosMembro.Criar(requisicao.Dados!),
            aviso.Versao, relogio.Agora));
        await banco.SaveChangesAsync(cancelamento);
        return Results.Ok(new { mensagem = "Recebemos seus dados. A igreja analisará o cadastro." });
    }

    private static async Task<IResult> ObterResumoAsync(
        Guid? igrejaId, BancoContexto banco, UsuarioAtual usuarioAtual,
        CancellationToken cancelamento)
    {
        var (id, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        var ativos = await banco.Membros.CountAsync(x => x.IgrejaId == id && x.Situacao == "ATIVO", cancelamento);
        var inativos = await banco.Membros.CountAsync(x => x.IgrejaId == id && x.Situacao == "INATIVO", cancelamento);
        var pendentes = await banco.SolicitacoesMembros.CountAsync(
            x => x.IgrejaId == id && x.Estado == "PENDENTE", cancelamento);
        return Results.Ok(new { ativos, inativos, pendentes });
    }

    private static async Task<IResult> ListarAsync(
        Guid? igrejaId, string? termo, string? situacao, int pagina,
        BancoContexto banco, UsuarioAtual usuarioAtual, CancellationToken cancelamento)
    {
        var (id, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        if (pagina < 1) pagina = 1;
        if (pagina > 100_000) return Results.BadRequest();
        if (situacao is not null && situacao is not ("ATIVO" or "INATIVO"))
            return Results.BadRequest();
        var consulta = banco.Membros.AsNoTracking().Where(x => x.IgrejaId == id);
        if (situacao is not null) consulta = consulta.Where(x => x.Situacao == situacao);
        consulta = Filtrar(consulta, termo, banco);
        var total = await consulta.CountAsync(cancelamento);
        var itens = await consulta.OrderBy(x => x.Dados.Nome).ThenBy(x => x.Dados.Sobrenome)
            .ThenBy(x => x.Id).Skip((pagina - 1) * 20).Take(20)
            .Select(x => new { x.Id, x.Dados.Nome, x.Dados.Sobrenome,
                x.Dados.Whatsapp, x.Dados.EhMenor, x.Situacao, x.AtualizadoEm })
            .ToListAsync(cancelamento);
        return Results.Ok(new { itens, total, pagina, tamanhoPagina = 20 });
    }

    private static async Task<IResult> ListarSolicitacoesAsync(
        Guid? igrejaId, string? estado, int pagina,
        BancoContexto banco, UsuarioAtual usuarioAtual, CancellationToken cancelamento)
    {
        var (id, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        if (pagina < 1) pagina = 1;
        if (pagina > 100_000) return Results.BadRequest();
        estado ??= "PENDENTE";
        if (estado is not ("PENDENTE" or "APROVADA" or "RECUSADA")) return Results.BadRequest();
        var consulta = banco.SolicitacoesMembros.AsNoTracking()
            .Where(x => x.IgrejaId == id && x.Estado == estado);
        var total = await consulta.CountAsync(cancelamento);
        var itens = await consulta.OrderByDescending(x => x.CriadaEm).ThenBy(x => x.Id)
            .Skip((pagina - 1) * 20).Take(20)
            .Select(x => new { x.Id, x.Dados.Nome, x.Dados.Sobrenome,
                x.Dados.Whatsapp, x.Estado, x.CriadaEm })
            .ToListAsync(cancelamento);
        return Results.Ok(new { itens, total, pagina, tamanhoPagina = 20 });
    }

    private static async Task<IResult> ObterMembroAsync(
        Guid id, Guid? igrejaId, BancoContexto banco,
        UsuarioAtual usuarioAtual, CancellationToken cancelamento)
    {
        var (igreja, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        var membro = await banco.Membros.AsNoTracking().SingleOrDefaultAsync(
            x => x.Id == id && x.IgrejaId == igreja, cancelamento);
        if (membro is null) return Results.NotFound();
        var historico = await ObterHistoricoAsync(banco, igreja, id, null, cancelamento);
        return Results.Ok(new { membro.Id, membro.IgrejaId, membro.Dados,
            membro.Situacao, membro.Origem, membro.DataIngresso,
            membro.ObservacaoPastoral, membro.MotivoInativacao,
            membro.CriadoEm, membro.AtualizadoEm, historico });
    }

    private static async Task<IResult> ObterSolicitacaoAsync(
        Guid id, Guid? igrejaId, BancoContexto banco,
        UsuarioAtual usuarioAtual, CancellationToken cancelamento)
    {
        var (igreja, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        var solicitacao = await banco.SolicitacoesMembros.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && x.IgrejaId == igreja, cancelamento);
        if (solicitacao is null) return Results.NotFound();
        var historico = await ObterHistoricoAsync(banco, igreja, null, id, cancelamento);
        return Results.Ok(new { solicitacao.Id, solicitacao.IgrejaId,
            solicitacao.Dados, solicitacao.Estado, solicitacao.AvisoPrivacidadeVersao,
            solicitacao.CriadaEm, solicitacao.AtualizadaEm, solicitacao.DecididaEm,
            solicitacao.MotivoRecusa, solicitacao.MembroId, historico });
    }

    private static async Task<IResult> CriarAsync(
        Guid? igrejaId, SalvarMembroRequisicao requisicao, BancoContexto banco,
        UsuarioAtual usuarioAtual, IRelogio relogio, CancellationToken cancelamento)
    {
        var (igreja, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        var usuario = (await usuarioAtual.ObterAsync(cancelamento))!;
        var erros = ValidarSalvar(requisicao, relogio);
        if (erros.Count > 0) return Results.ValidationProblem(erros);
        var dados = ValidacaoDadosMembro.Criar(requisicao.Dados!);
        var duplicados = await EncontrarDuplicadosAsync(banco, igreja, dados.Whatsapp, null, cancelamento);
        if (duplicados.Count > 0 && !requisicao.ConfirmarNovoApesarDuplicidade)
            return ConflitoDuplicidade(duplicados);

        var agora = relogio.Agora;
        var membro = Membro.Criar(igreja, dados, "INCLUSAO_INTERNA",
            requisicao.DataIngresso, requisicao.ObservacaoPastoral, agora);
        banco.Membros.Add(membro);
        banco.HistoricosMembros.Add(HistoricoMembro.Criar(igreja, membro.Id, null,
            usuario.Id, "CRIADO", "Cadastro interno realizado.", agora));
        await banco.SaveChangesAsync(cancelamento);
        return Results.Created($"/api/membros/{membro.Id}", new { membro.Id });
    }

    private static async Task<IResult> EditarAsync(
        Guid id, Guid? igrejaId, SalvarMembroRequisicao requisicao,
        BancoContexto banco, UsuarioAtual usuarioAtual, IRelogio relogio,
        CancellationToken cancelamento)
    {
        var (igreja, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        var membro = await banco.Membros.SingleOrDefaultAsync(
            x => x.Id == id && x.IgrejaId == igreja, cancelamento);
        if (membro is null) return Results.NotFound();
        var erros = ValidarSalvar(requisicao, relogio);
        if (erros.Count > 0) return Results.ValidationProblem(erros);
        var dados = ValidacaoDadosMembro.Criar(requisicao.Dados!);
        var duplicados = await EncontrarDuplicadosAsync(banco, igreja, dados.Whatsapp, id, cancelamento);
        if (duplicados.Count > 0 && !requisicao.ConfirmarNovoApesarDuplicidade)
            return ConflitoDuplicidade(duplicados);

        var agora = relogio.Agora;
        membro.Atualizar(dados, requisicao.DataIngresso, requisicao.ObservacaoPastoral, agora);
        banco.HistoricosMembros.Add(HistoricoMembro.Criar(igreja, id, null,
            (await usuarioAtual.ObterAsync(cancelamento))!.Id, "EDITADO",
            "Dados cadastrais atualizados.", agora));
        await banco.SaveChangesAsync(cancelamento);
        return Results.NoContent();
    }

    private static async Task<IResult> AlterarSituacaoAsync(
        Guid id, Guid? igrejaId, AlterarSituacaoMembroRequisicao requisicao,
        BancoContexto banco, UsuarioAtual usuarioAtual, IRelogio relogio,
        CancellationToken cancelamento)
    {
        var (igreja, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        var membro = await banco.Membros.SingleOrDefaultAsync(
            x => x.Id == id && x.IgrejaId == igreja, cancelamento);
        if (membro is null) return Results.NotFound();
        if (requisicao.Motivo?.Trim().Length > 500)
            return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["motivo"] = ["Use no máximo 500 caracteres."] });
        if ((membro.Situacao == "ATIVO") == requisicao.Ativo) return Results.NoContent();
        var agora = relogio.Agora;
        membro.AlterarSituacao(requisicao.Ativo, requisicao.Motivo, agora);
        banco.HistoricosMembros.Add(HistoricoMembro.Criar(igreja, id, null,
            (await usuarioAtual.ObterAsync(cancelamento))!.Id,
            requisicao.Ativo ? "REATIVADO" : "INATIVADO",
            requisicao.Ativo ? null : requisicao.Motivo, agora));
        await banco.SaveChangesAsync(cancelamento);
        return Results.NoContent();
    }

    private static async Task<IResult> CorrigirSolicitacaoAsync(
        Guid id, Guid? igrejaId, CorrigirSolicitacaoMembroRequisicao requisicao,
        BancoContexto banco, UsuarioAtual usuarioAtual, IRelogio relogio,
        CancellationToken cancelamento)
    {
        var (igreja, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        var solicitacao = await banco.SolicitacoesMembros.SingleOrDefaultAsync(
            x => x.Id == id && x.IgrejaId == igreja, cancelamento);
        if (solicitacao is null) return Results.NotFound();
        if (solicitacao.Estado != "PENDENTE") return Results.Conflict();
        var erros = ValidacaoDadosMembro.Validar(
            requisicao.Dados, false, DateOnly.FromDateTime(relogio.Agora.UtcDateTime));
        if (requisicao.Dados?.EhMenor == true)
            erros["ehMenor"] = ["Solicitações públicas são destinadas a adultos."];
        if (erros.Count > 0) return Results.ValidationProblem(erros);
        var agora = relogio.Agora;
        solicitacao.Corrigir(ValidacaoDadosMembro.Criar(requisicao.Dados!), agora);
        banco.HistoricosMembros.Add(HistoricoMembro.Criar(igreja, null, id,
            (await usuarioAtual.ObterAsync(cancelamento))!.Id,
            "SOLICITACAO_CORRIGIDA", null, agora));
        try { await banco.SaveChangesAsync(cancelamento); }
        catch (DbUpdateConcurrencyException) { return Results.Conflict(); }
        return Results.NoContent();
    }

    private static async Task<IResult> AprovarAsync(
        Guid id, Guid? igrejaId, AprovarSolicitacaoMembroRequisicao requisicao,
        BancoContexto banco, UsuarioAtual usuarioAtual, IRelogio relogio,
        CancellationToken cancelamento)
    {
        var (igreja, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        var solicitacao = await banco.SolicitacoesMembros.SingleOrDefaultAsync(
            x => x.Id == id && x.IgrejaId == igreja, cancelamento);
        if (solicitacao is null) return Results.NotFound();
        if (solicitacao.Estado != "PENDENTE") return Results.Conflict();
        if (requisicao.ObservacaoPastoral?.Trim().Length > 2_000)
            return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["observacaoPastoral"] = ["Use no máximo 2.000 caracteres."] });
        if (requisicao.DataIngresso > DateOnly.FromDateTime(relogio.Agora.UtcDateTime))
            return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["dataIngresso"] = ["A data de ingresso não pode ser futura."] });
        var autor = (await usuarioAtual.ObterAsync(cancelamento))!;
        var agora = relogio.Agora;
        Guid membroId;
        if (requisicao.MembroExistenteId is { } existenteId)
        {
            var existe = await banco.Membros.AnyAsync(
                x => x.Id == existenteId && x.IgrejaId == igreja, cancelamento);
            if (!existe) return Results.NotFound();
            membroId = existenteId;
        }
        else
        {
            var duplicados = await EncontrarDuplicadosAsync(
                banco, igreja, solicitacao.Dados.Whatsapp, null, cancelamento);
            if (duplicados.Count > 0 && !requisicao.ConfirmarNovoApesarDuplicidade)
                return ConflitoDuplicidade(duplicados);
            var membro = Membro.Criar(igreja, solicitacao.Dados,
                "FORMULARIO", requisicao.DataIngresso, requisicao.ObservacaoPastoral, agora);
            banco.Membros.Add(membro);
            membroId = membro.Id;
        }
        solicitacao.Aprovar(autor.Id, membroId, agora);
        banco.HistoricosMembros.Add(HistoricoMembro.Criar(igreja, membroId, id,
            autor.Id, requisicao.MembroExistenteId is null ? "SOLICITACAO_APROVADA" :
                "SOLICITACAO_VINCULADA", null, agora));
        try { await banco.SaveChangesAsync(cancelamento); }
        catch (DbUpdateConcurrencyException) { return Results.Conflict(); }
        return Results.Ok(new { membroId });
    }

    private static async Task<IResult> RecusarAsync(
        Guid id, Guid? igrejaId, RecusarSolicitacaoMembroRequisicao requisicao,
        BancoContexto banco, UsuarioAtual usuarioAtual, IRelogio relogio,
        CancellationToken cancelamento)
    {
        var (igreja, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        var solicitacao = await banco.SolicitacoesMembros.SingleOrDefaultAsync(
            x => x.Id == id && x.IgrejaId == igreja, cancelamento);
        if (solicitacao is null) return Results.NotFound();
        if (solicitacao.Estado != "PENDENTE") return Results.Conflict();
        if (string.IsNullOrWhiteSpace(requisicao.Motivo) || requisicao.Motivo.Trim().Length > 500)
            return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["motivo"] = ["Informe o motivo da recusa (até 500 caracteres)."] });
        var agora = relogio.Agora;
        var autor = (await usuarioAtual.ObterAsync(cancelamento))!;
        solicitacao.Recusar(autor.Id, requisicao.Motivo, agora);
        banco.HistoricosMembros.Add(HistoricoMembro.Criar(igreja, null, id,
            autor.Id, "SOLICITACAO_RECUSADA", requisicao.Motivo, agora));
        try { await banco.SaveChangesAsync(cancelamento); }
        catch (DbUpdateConcurrencyException) { return Results.Conflict(); }
        return Results.NoContent();
    }

    private static async Task<IResult> ObterQrCodeAsync(
        Guid? igrejaId, HttpRequest requisicao, HttpResponse resposta,
        IConfiguration configuracao, BancoContexto banco,
        UsuarioAtual usuarioAtual, CancellationToken cancelamento)
    {
        var (id, erro) = await IgrejaAutorizadaAsync(igrejaId, banco, usuarioAtual, cancelamento);
        if (erro is not null) return erro;
        var token = await banco.Igrejas.AsNoTracking().Where(x => x.Id == id)
            .Select(x => x.IdentificadorPublicoMembros).SingleAsync(cancelamento);
        var origem = configuracao["Aplicacao:UrlPublica"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(origem)) origem = $"{requisicao.Scheme}://{requisicao.Host}";
        var url = $"{origem}/membros/cadastro/{token}";
        using var qr = QRCodeGenerator.GenerateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        resposta.Headers.CacheControl = "no-store";
        return Results.Content(new SvgQRCode(qr).GetGraphic(8), "image/svg+xml");
    }

    private static async Task<(Guid Id, IResult? Erro)> IgrejaAutorizadaAsync(
        Guid? igrejaId, BancoContexto banco, UsuarioAtual usuarioAtual,
        CancellationToken cancelamento)
    {
        var usuario = await usuarioAtual.ObterAsync(cancelamento);
        if (usuario is null) return (Guid.Empty, Results.Unauthorized());
        if (usuario.Perfil == PerfilUsuario.Equipe) return (Guid.Empty, Results.Forbid());
        if (igrejaId is null) return (Guid.Empty, Results.BadRequest());
        if (usuario.Perfil == PerfilUsuario.Pastor && usuario.IgrejaId != igrejaId)
            return (Guid.Empty, Results.Forbid());
        if (!await banco.Igrejas.AnyAsync(x => x.Id == igrejaId, cancelamento))
            return (Guid.Empty, Results.NotFound());
        return (igrejaId.Value, null);
    }

    private static Dictionary<string, string[]> ValidarSalvar(
        SalvarMembroRequisicao requisicao, IRelogio relogio)
    {
        var hoje = DateOnly.FromDateTime(relogio.Agora.UtcDateTime);
        var erros = ValidacaoDadosMembro.Validar(requisicao.Dados, false, hoje);
        if (requisicao.DataIngresso > hoje)
            erros["dataIngresso"] = ["A data de ingresso não pode ser futura."];
        if (requisicao.ObservacaoPastoral?.Trim().Length > 2_000)
            erros["observacaoPastoral"] = ["Use no máximo 2.000 caracteres."];
        return erros;
    }

    private static IQueryable<Membro> Filtrar(
        IQueryable<Membro> consulta, string? termo, BancoContexto banco)
    {
        var pesquisa = termo?.Trim();
        if (string.IsNullOrWhiteSpace(pesquisa)) return consulta;
        var digitos = new string(pesquisa.Where(char.IsAsciiDigit).ToArray());
        return string.Equals(banco.Database.ProviderName,
            "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal)
            ? consulta.Where(x =>
                x.Dados.Nome.Contains(pesquisa, StringComparison.OrdinalIgnoreCase) ||
                x.Dados.Sobrenome.Contains(pesquisa, StringComparison.OrdinalIgnoreCase) ||
                (digitos.Length > 0 && x.Dados.Whatsapp != null && x.Dados.Whatsapp.Contains(digitos)))
            : consulta.Where(x => EF.Functions.ILike(x.Dados.Nome, $"%{pesquisa}%") ||
                EF.Functions.ILike(x.Dados.Sobrenome, $"%{pesquisa}%") ||
                (digitos.Length > 0 && x.Dados.Whatsapp != null && x.Dados.Whatsapp.Contains(digitos)));
    }

    private static async Task<List<object>> EncontrarDuplicadosAsync(
        BancoContexto banco, Guid igrejaId, string? whatsapp, Guid? ignorarId,
        CancellationToken cancelamento)
    {
        if (whatsapp is null) return [];
        var membros = await banco.Membros.AsNoTracking()
            .Where(x => x.IgrejaId == igrejaId && x.Dados.Whatsapp == whatsapp && x.Id != ignorarId)
            .Select(x => new { x.Id, x.Dados.Nome, x.Dados.Sobrenome, x.Situacao })
            .Take(10).ToListAsync(cancelamento);
        return membros.Cast<object>().ToList();
    }

    private static IResult ConflitoDuplicidade(List<object> membros) =>
        Results.Json(new { mensagem = "Há membro com o mesmo WhatsApp nesta igreja.",
            duplicados = membros }, statusCode: StatusCodes.Status409Conflict);

    private static async Task<object> ObterHistoricoAsync(
        BancoContexto banco, Guid igrejaId, Guid? membroId,
        Guid? solicitacaoId, CancellationToken cancelamento) =>
        await banco.HistoricosMembros.AsNoTracking()
            .Where(x => x.IgrejaId == igrejaId &&
                (membroId != null ? x.MembroId == membroId : x.SolicitacaoId == solicitacaoId))
            .OrderByDescending(x => x.CriadoEm)
            .Select(x => new { x.Acao, x.Descricao, x.CriadoEm,
                NomeAutor = x.Autor.Nome })
            .ToListAsync(cancelamento);
}
