using System.Net;
using System.Net.Http.Json;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Infraestrutura;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CrmIctm.Api.Tests.Integracao;

public sealed class CadastroMembrosTests
{
    private const string Senha = "Senha!Segura2026";

    [Fact]
    public async Task EnvioPublicoCriaSolicitacaoPendenteSemMembroAtivo()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        var cliente = CriarCliente(aplicacao);
        var resposta = await cliente.PostAsJsonAsync(
            $"/api/publico/membros/{igreja.IdentificadorPublicoMembros}/solicitacoes",
            new { dados = DadosAdulto(), declaracaoMaioridade = true,
                avisoPrivacidadeReconhecido = true, avisoPrivacidadeVersao = "membros-teste-1" });
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        await using var escopo = aplicacao.Services.CreateAsyncScope();
        var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
        var solicitacao = await banco.SolicitacoesMembros.SingleAsync();
        Assert.Equal(igreja.Id, solicitacao.IgrejaId);
        Assert.Equal("PENDENTE", solicitacao.Estado);
        Assert.Equal("membros-teste-1", solicitacao.AvisoPrivacidadeVersao);
        Assert.Equal("+5511987654321", solicitacao.Dados.Whatsapp);
        Assert.Empty(await banco.Membros.ToListAsync());
    }

    [Fact]
    public async Task FormularioPublicoRecusaMenorESemCienciaDePrivacidade()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        var cliente = CriarCliente(aplicacao);
        var resposta = await cliente.PostAsJsonAsync(
            $"/api/publico/membros/{igreja.IdentificadorPublicoMembros}/solicitacoes",
            new { dados = DadosAdulto(new DateOnly(2015, 1, 1)),
                declaracaoMaioridade = false, avisoPrivacidadeReconhecido = false,
                avisoPrivacidadeVersao = "membros-teste-1" });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        await using var escopo = aplicacao.Services.CreateAsyncScope();
        Assert.Empty(await escopo.ServiceProvider.GetRequiredService<BancoContexto>()
            .SolicitacoesMembros.ToListAsync());
    }

    [Fact]
    public async Task SemAvisoAprovadoFormularioNaoAceitaEnvio()
    {
        await using var aplicacao = new AplicacaoDeTeste(configurarAviso: false);
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        var cliente = CriarCliente(aplicacao);
        var endereco = $"/api/publico/membros/{igreja.IdentificadorPublicoMembros}";
        var formulario = await cliente.GetFromJsonAsync<FormularioResposta>(endereco);
        Assert.Null(formulario!.AvisoPrivacidade);
        var envio = await cliente.PostAsJsonAsync(endereco + "/solicitacoes",
            new { dados = DadosAdulto(), declaracaoMaioridade = true,
                avisoPrivacidadeReconhecido = true, avisoPrivacidadeVersao = "membros-teste-1" });
        Assert.Equal(HttpStatusCode.ServiceUnavailable, envio.StatusCode);
    }

    [Fact]
    public async Task PastorAprovaNaSuaIgrejaMasNaoAcessaOutraEEquipeNaoAcessa()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var igrejaA = await CriarIgrejaAsync(aplicacao.Services, "Igreja A");
        var igrejaB = await CriarIgrejaAsync(aplicacao.Services, "Igreja B");
        await CriarUsuarioAsync(aplicacao.Services, "Pastor", "pastor@teste.local",
            PerfilUsuario.Pastor, igrejaA.Id);
        await CriarUsuarioAsync(aplicacao.Services, "Equipe", "equipe@teste.local",
            PerfilUsuario.Equipe, igrejaA.Id);

        var publico = CriarCliente(aplicacao);
        Assert.Equal(HttpStatusCode.OK, (await publico.PostAsJsonAsync(
            $"/api/publico/membros/{igrejaA.IdentificadorPublicoMembros}/solicitacoes",
            new { dados = DadosAdulto(), declaracaoMaioridade = true,
                avisoPrivacidadeReconhecido = true, avisoPrivacidadeVersao = "membros-teste-1" })).StatusCode);
        Guid solicitacaoId;
        await using (var escopo = aplicacao.Services.CreateAsyncScope())
        {
            solicitacaoId = await escopo.ServiceProvider.GetRequiredService<BancoContexto>()
                .SolicitacoesMembros.Select(x => x.Id).SingleAsync();
        }

        var equipe = CriarCliente(aplicacao);
        await EntrarAsync(equipe, "equipe@teste.local");
        Assert.Equal(HttpStatusCode.Forbidden,
            (await equipe.GetAsync($"/api/membros/resumo?igrejaId={igrejaA.Id}")).StatusCode);

        var pastor = CriarCliente(aplicacao);
        await EntrarAsync(pastor, "pastor@teste.local");
        Assert.Equal(HttpStatusCode.Forbidden,
            (await pastor.GetAsync($"/api/membros/solicitacoes/{solicitacaoId}?igrejaId={igrejaB.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await pastor.GetAsync($"/api/membros/solicitacoes/{solicitacaoId}")).StatusCode);
        var aprovacao = await EnviarComCsrfAsync(pastor, HttpMethod.Post,
            $"/api/membros/solicitacoes/{solicitacaoId}/aprovar?igrejaId={igrejaA.Id}",
            new { membroExistenteId = (Guid?)null, confirmarNovoApesarDuplicidade = false,
                dataIngresso = (DateOnly?)null, observacaoPastoral = (string?)null });
        Assert.Equal(HttpStatusCode.OK, aprovacao.StatusCode);
        await using var escopoFinal = aplicacao.Services.CreateAsyncScope();
        var banco = escopoFinal.ServiceProvider.GetRequiredService<BancoContexto>();
        Assert.Equal("APROVADA", (await banco.SolicitacoesMembros.SingleAsync()).Estado);
        Assert.Equal("ATIVO", (await banco.Membros.SingleAsync()).Situacao);
        Assert.Single(await banco.HistoricosMembros.ToListAsync());
    }

    [Fact]
    public async Task InclusaoManualSinalizaDuplicidadeSemBloquearDecisaoExplicita()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        await CriarUsuarioAsync(aplicacao.Services, "Pastor", "pastor@teste.local",
            PerfilUsuario.Pastor, igreja.Id);
        var pastor = CriarCliente(aplicacao);
        await EntrarAsync(pastor, "pastor@teste.local");
        var endereco = $"/api/membros?igrejaId={igreja.Id}";
        var primeiro = await EnviarComCsrfAsync(pastor, HttpMethod.Post, endereco,
            new { dados = DadosAdulto(), confirmarNovoApesarDuplicidade = false });
        Assert.Equal(HttpStatusCode.Created, primeiro.StatusCode);
        var conflito = await EnviarComCsrfAsync(pastor, HttpMethod.Post, endereco,
            new { dados = DadosAdulto(), confirmarNovoApesarDuplicidade = false });
        Assert.Equal(HttpStatusCode.Conflict, conflito.StatusCode);
        var resposta = await EnviarComCsrfAsync(pastor, HttpMethod.Post, endereco,
            new { dados = DadosAdulto(), confirmarNovoApesarDuplicidade = true });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var lista = await pastor.GetFromJsonAsync<PaginaResposta>(endereco + "&termo=Maria&pagina=1");
        Assert.Equal(2, lista!.Total);
    }

    [Fact]
    public async Task MenorExigeResponsavelLegalENaoPrecisaTerWhatsappProprio()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        await CriarUsuarioAsync(aplicacao.Services, "Pastor", "pastor@teste.local",
            PerfilUsuario.Pastor, igreja.Id);
        var pastor = CriarCliente(aplicacao);
        await EntrarAsync(pastor, "pastor@teste.local");
        var endereco = $"/api/membros?igrejaId={igreja.Id}";
        var semResponsavel = await EnviarComCsrfAsync(pastor, HttpMethod.Post, endereco,
            new { dados = new { nome = "Criança", sobrenome = "Teste", ehMenor = true } });
        Assert.Equal(HttpStatusCode.BadRequest, semResponsavel.StatusCode);
        var comResponsavel = await EnviarComCsrfAsync(pastor, HttpMethod.Post, endereco,
            new { dados = new { nome = "Criança", sobrenome = "Teste", ehMenor = true,
                nomeResponsavelLegal = "Responsável Teste",
                whatsappResponsavelLegal = "(11) 98765-4321",
                vinculoResponsavelLegal = "Mãe" } });
        Assert.Equal(HttpStatusCode.Created, comResponsavel.StatusCode);
        await using var escopo = aplicacao.Services.CreateAsyncScope();
        var membro = await escopo.ServiceProvider.GetRequiredService<BancoContexto>()
            .Membros.SingleAsync();
        Assert.Null(membro.Dados.Whatsapp);
        Assert.Equal("+5511987654321", membro.Dados.WhatsappResponsavelLegal);
    }

    [Fact]
    public async Task AdministradorDeveEscolherVinculoOuNovoAoEncontrarDuplicidade()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var igrejaA = await CriarIgrejaAsync(aplicacao.Services, "Igreja A");
        var igrejaB = await CriarIgrejaAsync(aplicacao.Services, "Igreja B");
        await CriarUsuarioAsync(aplicacao.Services, "Admin", "admin@teste.local",
            PerfilUsuario.Administrador, null);
        var admin = CriarCliente(aplicacao);
        await EntrarAsync(admin, "admin@teste.local");

        var criacao = await EnviarComCsrfAsync(admin, HttpMethod.Post,
            $"/api/membros?igrejaId={igrejaA.Id}",
            new { dados = DadosAdulto(), confirmarNovoApesarDuplicidade = false });
        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);
        var existente = (await criacao.Content.ReadFromJsonAsync<IdResposta>())!.Id;

        var publico = CriarCliente(aplicacao);
        var envio = await publico.PostAsJsonAsync(
            $"/api/publico/membros/{igrejaA.IdentificadorPublicoMembros}/solicitacoes",
            new { dados = DadosAdulto(), declaracaoMaioridade = true,
                avisoPrivacidadeReconhecido = true, avisoPrivacidadeVersao = "membros-teste-1" });
        Assert.Equal(HttpStatusCode.OK, envio.StatusCode);
        Guid solicitacaoId;
        await using (var escopo = aplicacao.Services.CreateAsyncScope())
            solicitacaoId = await escopo.ServiceProvider.GetRequiredService<BancoContexto>()
                .SolicitacoesMembros.Select(x => x.Id).SingleAsync();

        Assert.Equal(HttpStatusCode.NotFound,
            (await admin.GetAsync($"/api/membros/solicitacoes/{solicitacaoId}?igrejaId={igrejaB.Id}")).StatusCode);
        var endereco = $"/api/membros/solicitacoes/{solicitacaoId}/aprovar?igrejaId={igrejaA.Id}";
        var conflito = await EnviarComCsrfAsync(admin, HttpMethod.Post, endereco,
            new { membroExistenteId = (Guid?)null, confirmarNovoApesarDuplicidade = false });
        Assert.Equal(HttpStatusCode.Conflict, conflito.StatusCode);
        var vinculo = await EnviarComCsrfAsync(admin, HttpMethod.Post, endereco,
            new { membroExistenteId = existente, confirmarNovoApesarDuplicidade = false });
        Assert.Equal(HttpStatusCode.OK, vinculo.StatusCode);
        await using var escopoFinal = aplicacao.Services.CreateAsyncScope();
        var banco = escopoFinal.ServiceProvider.GetRequiredService<BancoContexto>();
        Assert.Single(await banco.Membros.ToListAsync());
        Assert.Equal(existente, (await banco.SolicitacoesMembros.SingleAsync()).MembroId);
    }

    private static object DadosAdulto(DateOnly? nascimento = null) => new
    {
        nome = "Maria", sobrenome = "da Silva", whatsapp = "(11) 98765-4321",
        email = "maria@teste.local", dataNascimento = nascimento,
        situacaoBatismo = "NAO_INFORMADO", ehMenor = false
    };

    private static async Task<Igreja> CriarIgrejaAsync(IServiceProvider servicos,
        string nome = "Igreja de teste")
    {
        await using var escopo = servicos.CreateAsyncScope();
        var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
        var igreja = Igreja.Criar(nome, "Rua das Flores", "123", "Centro", null,
            "98910000", "Três de Maio", "RS", DateTimeOffset.UtcNow);
        banco.Igrejas.Add(igreja);
        await banco.SaveChangesAsync();
        return igreja;
    }

    private static async Task CriarUsuarioAsync(IServiceProvider servicos, string nome,
        string email, PerfilUsuario perfil, Guid? igrejaId)
    {
        await using var escopo = servicos.CreateAsyncScope();
        var gerenciador = escopo.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var agora = DateTimeOffset.UtcNow;
        var usuario = new Usuario { Id = Guid.NewGuid(), Nome = nome,
            UserName = email, Email = email, EmailConfirmed = true,
            LockoutEnabled = true, Perfil = perfil, IgrejaId = igrejaId,
            Ativo = true, DeveTrocarSenha = false,
            CriadoEm = agora, AtualizadoEm = agora };
        var resultado = await gerenciador.CreateAsync(usuario, Senha);
        Assert.True(resultado.Succeeded);
    }

    private static HttpClient CriarCliente(AplicacaoDeTeste aplicacao) =>
        aplicacao.CreateClient(new WebApplicationFactoryClientOptions
            { BaseAddress = new Uri("https://localhost") });

    private static async Task EntrarAsync(HttpClient cliente, string email)
    {
        var resposta = await EnviarComCsrfAsync(cliente, HttpMethod.Post,
            "/api/autenticacao/entrar", new { email, senha = Senha });
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    private static async Task<HttpResponseMessage> EnviarComCsrfAsync(
        HttpClient cliente, HttpMethod metodo, string endereco, object corpo)
    {
        var token = await cliente.GetFromJsonAsync<TokenCsrf>("/api/seguranca/token-csrf");
        using var mensagem = new HttpRequestMessage(metodo, endereco)
            { Content = JsonContent.Create(corpo) };
        mensagem.Headers.Add("X-CSRF-TOKEN", token!.Token);
        return await cliente.SendAsync(mensagem);
    }

    private sealed record TokenCsrf(string Token);
    private sealed record PaginaResposta(int Total);
    private sealed record IdResposta(Guid Id);
    private sealed record FormularioResposta(object? AvisoPrivacidade);

    private sealed class AplicacaoDeTeste : WebApplicationFactory<Program>
    {
        private readonly bool _configurarAviso;
        private readonly string _nomeBanco = $"crm-ictm-membros-{Guid.NewGuid()}";

        public AplicacaoDeTeste(bool configurarAviso = true) => _configurarAviso = configurarAviso;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            if (_configurarAviso)
                builder.ConfigureAppConfiguration((_, configuracao) =>
                    configuracao.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["AvisoPrivacidadeMembros:Versao"] = "membros-teste-1",
                        ["AvisoPrivacidadeMembros:Texto"] = "Aviso somente para testes."
                    }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<BancoContexto>>();
                services.RemoveAll<IDbContextOptionsConfiguration<BancoContexto>>();
                services.RemoveAll<BancoContexto>();
                services.AddDbContext<BancoContexto>(opcoes =>
                    opcoes.UseInMemoryDatabase(_nomeBanco));
            });
        }
    }
}
