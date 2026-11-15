using System.Net;
using System.Net.Http.Json;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Infraestrutura;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CrmIctm.Api.Tests.Integracao;

public sealed class AniversariantesApiTests
{
    private const string Senha = "Senha!Segura2026";

    /// <summary>Domingo, 15 de novembro de 2026, 9h em Brasília.</summary>
    private static readonly DateTimeOffset Domingo =
        new(2026, 11, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task EquipeNaoAlcancaAniversariantes()
    {
        await using var aplicacao = new AplicacaoDeTeste(Domingo);
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        await CriarUsuarioAsync(aplicacao.Services, "Equipe", "equipe@teste.local",
            PerfilUsuario.Equipe, igreja.Id);
        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, "equipe@teste.local");

        var lista = await cliente.GetAsync($"/api/membros/aniversariantes?igrejaId={igreja.Id}");
        var contadores = await cliente.GetAsync(
            $"/api/membros/aniversariantes/contadores?igrejaId={igreja.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, lista.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, contadores.StatusCode);
    }

    [Fact]
    public async Task PastorNaoAlcancaAniversariantesDeOutraIgreja()
    {
        await using var aplicacao = new AplicacaoDeTeste(Domingo);
        var igrejaA = await CriarIgrejaAsync(aplicacao.Services, "Igreja A");
        var igrejaB = await CriarIgrejaAsync(aplicacao.Services, "Igreja B");
        await CriarUsuarioAsync(aplicacao.Services, "Pastor", "pastor@teste.local",
            PerfilUsuario.Pastor, igrejaA.Id);
        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, "pastor@teste.local");

        var propria = await cliente.GetAsync($"/api/membros/aniversariantes?igrejaId={igrejaA.Id}");
        var alheia = await cliente.GetAsync($"/api/membros/aniversariantes?igrejaId={igrejaB.Id}");

        Assert.Equal(HttpStatusCode.OK, propria.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, alheia.StatusCode);
    }

    [Fact]
    public async Task PeriodoDesconhecidoResponde400()
    {
        await using var aplicacao = new AplicacaoDeTeste(Domingo);
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        var cliente = await PastorDeAsync(aplicacao, igreja);

        var resposta = await cliente.GetAsync(
            $"/api/membros/aniversariantes?igrejaId={igreja.Id}&periodo=ANO");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task ListaDeHojeTrazSomenteAtivosComDataDeNascimento()
    {
        await using var aplicacao = new AplicacaoDeTeste(Domingo);
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Ana",
            new DateOnly(1992, 11, 15));
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Bruno",
            new DateOnly(1992, 11, 16));
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Carla", null);
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Davi",
            new DateOnly(1992, 11, 15), ativo: false);
        var cliente = await PastorDeAsync(aplicacao, igreja);

        var lista = await cliente.GetFromJsonAsync<ListaResposta>(
            $"/api/membros/aniversariantes?igrejaId={igreja.Id}&periodo=HOJE");

        var item = Assert.Single(lista!.Itens);
        Assert.Equal("Ana", item.Nome);
        Assert.Equal(new DateOnly(2026, 11, 15), item.DataOcorrencia);
        Assert.Equal(34, item.IdadeQueCompleta);
        Assert.Equal("MEMBRO", item.Destino);
        Assert.Equal("+5541998124455", item.WhatsappDestino);
        Assert.Equal(new DateOnly(2026, 11, 15), lista.Hoje);
    }

    [Fact]
    public async Task SemanaQueViraOAnoVemNaOrdemDoCalendario()
    {
        // Quinta, 31 de dezembro de 2026: a semana vai de 27/12 a 02/01.
        await using var aplicacao = new AplicacaoDeTeste(
            new DateTimeOffset(2026, 12, 31, 12, 0, 0, TimeSpan.Zero));
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Janeiro",
            new DateOnly(1990, 1, 1));
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Dezembro",
            new DateOnly(1990, 12, 31));
        var cliente = await PastorDeAsync(aplicacao, igreja);

        var lista = await cliente.GetFromJsonAsync<ListaResposta>(
            $"/api/membros/aniversariantes?igrejaId={igreja.Id}&periodo=SEMANA");

        Assert.Equal(new DateOnly(2026, 12, 27), lista!.Inicio);
        Assert.Equal(new DateOnly(2027, 1, 2), lista.Fim);
        Assert.Equal(["Dezembro", "Janeiro"], lista.Itens.Select(x => x.Nome));
        Assert.Equal(new DateOnly(2026, 12, 31), lista.Itens[0].DataOcorrencia);
        Assert.Equal(new DateOnly(2027, 1, 1), lista.Itens[1].DataOcorrencia);
        Assert.Equal(36, lista.Itens[0].IdadeQueCompleta);
        Assert.Equal(37, lista.Itens[1].IdadeQueCompleta);
    }

    [Fact]
    public async Task NascidoEm29DeFevereiroApareceNoDia28EmAnoComum()
    {
        await using var aplicacao = new AplicacaoDeTeste(
            new DateTimeOffset(2027, 2, 28, 12, 0, 0, TimeSpan.Zero));
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Bissexta",
            new DateOnly(2000, 2, 29));
        var cliente = await PastorDeAsync(aplicacao, igreja);

        var lista = await cliente.GetFromJsonAsync<ListaResposta>(
            $"/api/membros/aniversariantes?igrejaId={igreja.Id}&periodo=HOJE");

        var item = Assert.Single(lista!.Itens);
        Assert.Equal(new DateOnly(2000, 2, 29), item.DataNascimento);
        Assert.Equal(new DateOnly(2027, 2, 28), item.DataOcorrencia);
        Assert.Equal(27, item.IdadeQueCompleta);
    }

    [Fact]
    public async Task MenorSemWhatsappProprioFalaComOResponsavel()
    {
        await using var aplicacao = new AplicacaoDeTeste(Domingo);
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Davi",
            new DateOnly(2012, 11, 15), whatsapp: null, ehMenor: true,
            whatsappResponsavel: "+5541996541120", nomeResponsavel: "Sônia Lima");
        var cliente = await PastorDeAsync(aplicacao, igreja);

        var lista = await cliente.GetFromJsonAsync<ListaResposta>(
            $"/api/membros/aniversariantes?igrejaId={igreja.Id}&periodo=HOJE");

        var item = Assert.Single(lista!.Itens);
        Assert.Equal("RESPONSAVEL", item.Destino);
        Assert.Equal("+5541996541120", item.WhatsappDestino);
        Assert.Equal("Sônia Lima", item.NomeResponsavel);
        Assert.Equal("mãe", item.VinculoResponsavel);
        Assert.True(item.EhMenor);
    }

    [Fact]
    public async Task AdultoSemWhatsappApareceSemDestino()
    {
        await using var aplicacao = new AplicacaoDeTeste(Domingo);
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Marcos",
            new DateOnly(1974, 11, 15), whatsapp: null);
        var cliente = await PastorDeAsync(aplicacao, igreja);

        var lista = await cliente.GetFromJsonAsync<ListaResposta>(
            $"/api/membros/aniversariantes?igrejaId={igreja.Id}&periodo=HOJE");

        var item = Assert.Single(lista!.Itens);
        Assert.Equal("NENHUM", item.Destino);
        Assert.Null(item.WhatsappDestino);
    }

    [Fact]
    public async Task ContadoresSeparamHojeSemanaMesESemData()
    {
        await using var aplicacao = new AplicacaoDeTeste(Domingo);
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        // Semana de 15/11 a 21/11; mês de novembro.
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Hoje",
            new DateOnly(1990, 11, 15));
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Quinta",
            new DateOnly(1990, 11, 19));
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "FimDoMes",
            new DateOnly(1990, 11, 29));
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "OutroMes",
            new DateOnly(1990, 12, 3));
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "SemData", null);
        await AdicionarMembroAsync(aplicacao.Services, igreja.Id, "Inativo",
            new DateOnly(1990, 11, 15), ativo: false);
        var cliente = await PastorDeAsync(aplicacao, igreja);

        var contadores = await cliente.GetFromJsonAsync<ContadoresResposta>(
            $"/api/membros/aniversariantes/contadores?igrejaId={igreja.Id}");

        Assert.Equal(1, contadores!.Hoje);
        Assert.Equal(2, contadores.Semana);
        Assert.Equal(3, contadores.Mes);
        Assert.Equal(1, contadores.SemDataNascimento);
        Assert.Equal(new DateOnly(2026, 11, 15), contadores.DataDeHoje);
    }

    private static async Task<HttpClient> PastorDeAsync(AplicacaoDeTeste aplicacao, Igreja igreja)
    {
        await CriarUsuarioAsync(aplicacao.Services, "Pastor", "pastor@teste.local",
            PerfilUsuario.Pastor, igreja.Id);
        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, "pastor@teste.local");
        return cliente;
    }

    private static async Task AdicionarMembroAsync(
        IServiceProvider servicos, Guid igrejaId, string nome, DateOnly? nascimento,
        string? whatsapp = "+5541998124455", bool ativo = true, bool ehMenor = false,
        string? whatsappResponsavel = null, string? nomeResponsavel = null)
    {
        await using var escopo = servicos.CreateAsyncScope();
        var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
        var agora = DateTimeOffset.UtcNow;
        var dados = DadosCadastraisMembro.Criar(
            nome, "de Teste", whatsapp, null, nascimento, null, null, null, null,
            null, null, null, "NAO_INFORMADO", null, ehMenor, nomeResponsavel,
            whatsappResponsavel, ehMenor ? "mãe" : null);
        var membro = Membro.Criar(igrejaId, dados, "EQUIPE", null, null, agora);
        if (!ativo) membro.AlterarSituacao(false, "Mudou de cidade.", agora);
        banco.Membros.Add(membro);
        await banco.SaveChangesAsync();
    }

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
        var token = await cliente.GetFromJsonAsync<TokenCsrf>("/api/seguranca/token-csrf");
        using var mensagem = new HttpRequestMessage(HttpMethod.Post, "/api/autenticacao/entrar")
            { Content = JsonContent.Create(new { email, senha = Senha }) };
        mensagem.Headers.Add("X-CSRF-TOKEN", token!.Token);
        var resposta = await cliente.SendAsync(mensagem);
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    private sealed record TokenCsrf(string Token);

    private sealed record ListaResposta(
        List<ItemResposta> Itens, DateOnly Hoje, DateOnly Inicio, DateOnly Fim, string Periodo);

    private sealed record ItemResposta(
        Guid Id, string Nome, string Sobrenome, string Funcao, bool EhMenor,
        DateOnly DataNascimento, DateOnly DataOcorrencia, int IdadeQueCompleta,
        string Destino, string? WhatsappDestino, string? NomeResponsavel,
        string? VinculoResponsavel);

    private sealed record ContadoresResposta(
        int Hoje, int Semana, int Mes, int SemDataNascimento, DateOnly DataDeHoje);

    private sealed class RelogioFixo(DateTimeOffset agora) : IRelogio
    {
        public DateTimeOffset Agora { get; } = agora;
    }

    private sealed class AplicacaoDeTeste(DateTimeOffset agora) : WebApplicationFactory<Program>
    {
        private readonly string _nomeBanco = $"crm-ictm-aniversarios-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<BancoContexto>>();
                services.RemoveAll<IDbContextOptionsConfiguration<BancoContexto>>();
                services.RemoveAll<BancoContexto>();
                services.AddDbContext<BancoContexto>(opcoes =>
                    opcoes.UseInMemoryDatabase(_nomeBanco));
                services.RemoveAll<IRelogio>();
                services.AddSingleton<IRelogio>(new RelogioFixo(agora));
            });
        }
    }
}
