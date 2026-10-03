using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
using PdfSharp.Pdf.IO;

namespace CrmIctm.Api.Tests.Integracao;

public sealed class GestaoAdministrativaTests
{
    private const string SenhaAdministrador = "Administrador!2026";

    [Fact]
    public async Task ConvitePdfDeveSerA4DeUmaPaginaComDestinoPermanente()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var (administrador, igreja) = await CriarAdministradorEIgrejaAsync(aplicacao.Services);
        var cliente = CriarCliente(aplicacao);
        var anonimo = await cliente.GetAsync($"/api/igrejas/{igreja.Id}/convite.pdf");
        Assert.Equal(HttpStatusCode.Unauthorized, anonimo.StatusCode);
        await EntrarAsync(cliente, administrador.Email!, SenhaAdministrador);

        var resposta = await cliente.GetAsync($"/api/igrejas/{igreja.Id}/convite.pdf");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("application/pdf", resposta.Content.Headers.ContentType!.MediaType);
        Assert.Equal("inline", resposta.Content.Headers.ContentDisposition!.DispositionType);
        Assert.True(resposta.Headers.CacheControl!.NoStore);
        using var memoria = new MemoryStream(await resposta.Content.ReadAsByteArrayAsync());
        using var documento = PdfReader.Open(memoria, PdfDocumentOpenMode.Import);
        Assert.Equal(1, documento.PageCount);
        var pagina = documento.Pages[0];
        Assert.Equal(210, pagina.Width.Millimeter, 2);
        Assert.Equal(297, pagina.Height.Millimeter, 2);
        var anotacao = Assert.Single(pagina.Annotations.Cast<PdfSharp.Pdf.Annotations.PdfAnnotation>());
        Assert.Equal($"https://localhost/visita/{igreja.IdentificadorPublico}",
            anotacao.Elements.GetDictionary("/A")!.Elements.GetString("/URI"));
    }

    [Theory]
    [InlineData(PerfilUsuario.Pastor)]
    [InlineData(PerfilUsuario.Equipe)]
    public async Task ConvitePdfDeveRespeitarIgrejaDoUsuario(PerfilUsuario perfil)
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var igreja = CriarIgreja("Igreja autorizada");
        var outraIgreja = CriarIgreja("Outra igreja");
        await using (var escopo = aplicacao.Services.CreateAsyncScope())
        {
            var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
            banco.Igrejas.AddRange(igreja, outraIgreja);
            await banco.SaveChangesAsync();
        }
        await CriarUsuarioAsync(aplicacao.Services, "Usuário do convite", "convite@teste.local",
            "Convite!2026Aa", perfil, igreja.Id, deveTrocarSenha: false);
        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, "convite@teste.local", "Convite!2026Aa");
        Assert.Equal(HttpStatusCode.NotFound,
            (await cliente.GetAsync($"/api/igrejas/{outraIgreja.Id}/convite.pdf")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await cliente.GetAsync($"/api/igrejas/{igreja.Id}/convite.pdf")).StatusCode);
    }

    [Fact]
    public async Task ConvitePdfNaoDeveAceitarIgrejaInativaOuInexistente()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var (administrador, igreja) = await CriarAdministradorEIgrejaAsync(aplicacao.Services);
        await using (var escopo = aplicacao.Services.CreateAsyncScope())
        {
            var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
            var registro = await banco.Igrejas.SingleAsync(x => x.Id == igreja.Id);
            registro.AlterarSituacao(false, DateTimeOffset.UtcNow);
            await banco.SaveChangesAsync();
        }
        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, administrador.Email!, SenhaAdministrador);
        Assert.Equal(HttpStatusCode.NotFound,
            (await cliente.GetAsync($"/api/igrejas/{igreja.Id}/convite.pdf")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await cliente.GetAsync($"/api/igrejas/{Guid.NewGuid()}/convite.pdf")).StatusCode);
    }

    [Fact]
    public async Task ConvitePdfDeveAcomodarNomeEEnderecoLongosSemCriarOutraPagina()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var (administrador, igreja) = await CriarAdministradorEIgrejaAsync(aplicacao.Services);
        await using (var escopo = aplicacao.Services.CreateAsyncScope())
        {
            var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
            var registro = await banco.Igrejas.SingleAsync(x => x.Id == igreja.Id);
            registro.AtualizarDados(new string('M', 150), new string('M', 150), new string('M', 20),
                new string('M', 100), new string('M', 100), "98910000", new string('M', 100), "RS",
                DateTimeOffset.UtcNow);
            await banco.SaveChangesAsync();
        }
        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, administrador.Email!, SenhaAdministrador);
        var resposta = await cliente.GetAsync($"/api/igrejas/{igreja.Id}/convite.pdf");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var memoria = new MemoryStream(await resposta.Content.ReadAsByteArrayAsync());
        using var documento = PdfReader.Open(memoria, PdfDocumentOpenMode.Import);
        Assert.Equal(1, documento.PageCount);
    }

    [Fact]
    public async Task AdministradorDeveCriarIgrejaComEnderecoEAuditoria()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        await CriarAdministradorAsync(aplicacao.Services);
        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, "admin@teste.local", SenhaAdministrador);

        var resposta = await EnviarComCsrfAsync(
            cliente,
            HttpMethod.Post,
            "/api/igrejas",
            new
            {
                nome = "Igreja Bairro Novo",
                logradouro = "Avenida Central",
                numero = "S/N",
                bairro = "Centro",
                complemento = "Sala 2",
                cep = "98910-000",
                cidade = "Três de Maio",
                estado = "RS"
            });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        await using var escopo = aplicacao.Services.CreateAsyncScope();
        var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
        var igreja = await banco.Igrejas.SingleAsync();
        Assert.Equal("98910000", igreja.Cep);
        Assert.Equal("S/N", igreja.Numero);
        Assert.NotEmpty(igreja.IdentificadorPublico);
        Assert.Contains(
            await banco.RegistrosAuditoriaAdministrativa.ToListAsync(),
            x => x.EntidadeId == igreja.Id && x.Acao == "CRIADA");
    }

    [Fact]
    public async Task SenhaDeveSerArmazenadaComoHashETrocadaNoPrimeiroAcesso()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var (administrador, igreja) = await CriarAdministradorEIgrejaAsync(aplicacao.Services);
        var clienteAdministrador = CriarCliente(aplicacao);
        await EntrarAsync(clienteAdministrador, administrador.Email!, SenhaAdministrador);
        const string senhaTemporaria = "Temporaria!2026Aa";

        var criacao = await EnviarComCsrfAsync(
            clienteAdministrador,
            HttpMethod.Post,
            "/api/usuarios",
            new
            {
                nome = "Usuário de Teste",
                email = "usuario@teste.local",
                perfil = "EQUIPE",
                igrejaId = igreja.Id,
                senhaTemporaria
            });
        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);

        Guid usuarioId;
        await using (var escopo = aplicacao.Services.CreateAsyncScope())
        {
            var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
            var usuario = await banco.Users.SingleAsync(x => x.Email == "usuario@teste.local");
            usuarioId = usuario.Id;
            Assert.NotEqual(senhaTemporaria, usuario.PasswordHash);
            Assert.True(usuario.DeveTrocarSenha);
        }

        var clienteUsuario = CriarCliente(aplicacao);
        await EntrarAsync(clienteUsuario, "usuario@teste.local", senhaTemporaria);
        var acessoAntesDaTroca = await clienteUsuario.GetAsync("/api/igrejas");
        Assert.Equal(HttpStatusCode.Unauthorized, acessoAntesDaTroca.StatusCode);

        var troca = await EnviarComCsrfAsync(
            clienteUsuario,
            HttpMethod.Post,
            "/api/autenticacao/alterar-senha",
            new
            {
                senhaAtual = senhaTemporaria,
                novaSenha = "NovaSenha!2026Segura",
                confirmacaoSenha = "NovaSenha!2026Segura"
            });
        Assert.Equal(HttpStatusCode.NoContent, troca.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await clienteUsuario.GetAsync("/api/igrejas")).StatusCode);

        const string novaSenhaTemporaria = "Redefinida!2026Aa";
        var redefinicao = await EnviarComCsrfAsync(
            clienteAdministrador,
            HttpMethod.Post,
            $"/api/usuarios/{usuarioId}/redefinir-senha",
            new { senhaTemporaria = novaSenhaTemporaria });
        Assert.Equal(HttpStatusCode.NoContent, redefinicao.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await clienteUsuario.GetAsync("/api/igrejas")).StatusCode);

        var clienteComNovaSenha = CriarCliente(aplicacao);
        await EntrarAsync(clienteComNovaSenha, "usuario@teste.local", novaSenhaTemporaria);
        var sessao = await clienteComNovaSenha.GetFromJsonAsync<SessaoResposta>("/api/autenticacao/sessao");
        Assert.True(sessao!.DeveTrocarSenha);
    }

    [Fact]
    public async Task PastorNaoDeveCriarAdministradorENaoEntraComIgrejaInativa()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var (administrador, igreja) = await CriarAdministradorEIgrejaAsync(aplicacao.Services);
        await CriarUsuarioAsync(
            aplicacao.Services,
            "Pastor de Teste",
            "pastor@teste.local",
            "Pastor!Temporario2026",
            PerfilUsuario.Pastor,
            igreja.Id,
            deveTrocarSenha: false);

        var clientePastor = CriarCliente(aplicacao);
        await EntrarAsync(clientePastor, "pastor@teste.local", "Pastor!Temporario2026");
        var tentativa = await EnviarComCsrfAsync(
            clientePastor,
            HttpMethod.Post,
            "/api/usuarios",
            new
            {
                nome = "Administrador indevido",
                email = "indevido@teste.local",
                perfil = "ADMINISTRADOR",
                igrejaId = (Guid?)null,
                senhaTemporaria = "Indevida!2026Aa"
            });
        Assert.Equal(HttpStatusCode.Forbidden, tentativa.StatusCode);

        var clienteAdministrador = CriarCliente(aplicacao);
        await EntrarAsync(clienteAdministrador, administrador.Email!, SenhaAdministrador);
        var desativacao = await EnviarComCsrfAsync(
            clienteAdministrador,
            HttpMethod.Put,
            $"/api/igrejas/{igreja.Id}/situacao",
            new { ativo = false });
        Assert.Equal(HttpStatusCode.NoContent, desativacao.StatusCode);

        var novoClientePastor = CriarCliente(aplicacao);
        var login = await EntrarAsync(
            novoClientePastor,
            "pastor@teste.local",
            "Pastor!Temporario2026",
            validarSucesso: false);
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task AdministradorNaoDeveAlterarAtendimentoForaDaIgrejaSelecionada()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var administrador = await CriarAdministradorAsync(aplicacao.Services);
        Guid igrejaSelecionadaId;
        Guid igrejaDoAtendimentoId;
        Guid atendimentoId;

        await using (var escopo = aplicacao.Services.CreateAsyncScope())
        {
            var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
            var igrejaSelecionada = CriarIgreja("Igreja selecionada");
            var igrejaDoAtendimento = CriarIgreja("Outra igreja");
            var atendimento = Atendimento.Criar(
                igrejaDoAtendimento.Id,
                "Visitante de outra igreja",
                "+5511987654321",
                true,
                false,
                "teste-1",
                DateTimeOffset.UtcNow);
            banco.Igrejas.AddRange(igrejaSelecionada, igrejaDoAtendimento);
            banco.Atendimentos.Add(atendimento);
            await banco.SaveChangesAsync();

            igrejaSelecionadaId = igrejaSelecionada.Id;
            igrejaDoAtendimentoId = igrejaDoAtendimento.Id;
            atendimentoId = atendimento.Id;
        }

        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, administrador.Email!, SenhaAdministrador);

        var semContexto = await cliente.GetAsync($"/api/atendimentos/{atendimentoId}");
        Assert.Equal(HttpStatusCode.BadRequest, semContexto.StatusCode);

        var detalheForaDoContexto = await cliente.GetAsync(
            $"/api/atendimentos/{atendimentoId}?igrejaId={igrejaSelecionadaId}");
        Assert.Equal(HttpStatusCode.NotFound, detalheForaDoContexto.StatusCode);

        var alteracaoForaDoContexto = await EnviarComCsrfAsync(
            cliente,
            HttpMethod.Post,
            $"/api/atendimentos/{atendimentoId}/observacoes?igrejaId={igrejaSelecionadaId}",
            new { texto = "Não deve ser salva." });
        Assert.Equal(HttpStatusCode.NotFound, alteracaoForaDoContexto.StatusCode);

        var alteracaoNoContextoCorreto = await EnviarComCsrfAsync(
            cliente,
            HttpMethod.Post,
            $"/api/atendimentos/{atendimentoId}/observacoes?igrejaId={igrejaDoAtendimentoId}",
            new { texto = "Observação da igreja correta." });
        Assert.Equal(HttpStatusCode.NoContent, alteracaoNoContextoCorreto.StatusCode);

        await using var escopoValidacao = aplicacao.Services.CreateAsyncScope();
        var bancoValidacao = escopoValidacao.ServiceProvider.GetRequiredService<BancoContexto>();
        var observacao = await bancoValidacao.ObservacoesAtendimento.SingleAsync();
        Assert.Equal("Observação da igreja correta.", observacao.Texto);
    }

    [Fact]
    public async Task PesquisaDeveLocalizarAtendimentoPorNomeOuWhatsapp()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var (administrador, igreja) = await CriarAdministradorEIgrejaAsync(aplicacao.Services);

        await using (var escopo = aplicacao.Services.CreateAsyncScope())
        {
            var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
            banco.Atendimentos.AddRange(
                Atendimento.Criar(
                    igreja.Id,
                    "Maria da Silva",
                    "+5555998765432",
                    true,
                    false,
                    "teste-1",
                    DateTimeOffset.UtcNow),
                Atendimento.Criar(
                    igreja.Id,
                    "João Pereira",
                    "+5555991112233",
                    false,
                    true,
                    "teste-1",
                    DateTimeOffset.UtcNow));
            await banco.SaveChangesAsync();
        }

        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, administrador.Email!, SenhaAdministrador);

        var porNome = await cliente.GetFromJsonAsync<JsonElement>(
            $"/api/atendimentos?igrejaId={igreja.Id}&termo=Maria");
        Assert.Single(porNome.EnumerateArray());
        Assert.Equal("Maria da Silva", porNome[0].GetProperty("nomeVisitante").GetString());

        var porTelefone = await cliente.GetFromJsonAsync<JsonElement>(
            $"/api/atendimentos?igrejaId={igreja.Id}&termo=9111-2233");
        Assert.Single(porTelefone.EnumerateArray());
        Assert.Equal("João Pereira", porTelefone[0].GetProperty("nomeVisitante").GetString());
    }

    [Fact]
    public async Task ApiNaoDevePermitirPularEtapas()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var (administrador, igreja) = await CriarAdministradorEIgrejaAsync(aplicacao.Services);
        Guid atendimentoId;

        await using (var escopo = aplicacao.Services.CreateAsyncScope())
        {
            var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
            var atendimento = Atendimento.Criar(
                igreja.Id,
                "Visitante em movimentação",
                "+5555998887766",
                true,
                false,
                "teste-1",
                DateTimeOffset.UtcNow);
            banco.Atendimentos.Add(atendimento);
            await banco.SaveChangesAsync();
            atendimentoId = atendimento.Id;
        }

        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, administrador.Email!, SenhaAdministrador);

        var salto = await EnviarComCsrfAsync(
            cliente,
            HttpMethod.Put,
            $"/api/atendimentos/{atendimentoId}/etapa?igrejaId={igreja.Id}",
            new { etapa = "EM_ATENDIMENTO", resultado = (string?)null });
        Assert.Equal(HttpStatusCode.BadRequest, salto.StatusCode);

        var movimentoAdjacente = await EnviarComCsrfAsync(
            cliente,
            HttpMethod.Put,
            $"/api/atendimentos/{atendimentoId}/etapa?igrejaId={igreja.Id}",
            new { etapa = "AGUARDANDO_CONTATO", resultado = (string?)null });
        Assert.Equal(HttpStatusCode.NoContent, movimentoAdjacente.StatusCode);

        await using var escopoValidacao = aplicacao.Services.CreateAsyncScope();
        var bancoValidacao = escopoValidacao.ServiceProvider.GetRequiredService<BancoContexto>();
        var atendimentoSalvo = await bancoValidacao.Atendimentos.SingleAsync(x => x.Id == atendimentoId);
        Assert.Equal(EtapaAtendimento.AguardandoContato, atendimentoSalvo.Etapa);
        Assert.Single(await bancoValidacao.HistoricosAtendimento.ToListAsync());
    }

    [Fact]
    public async Task DeveSalvarDadosComplementaresOpcionaisSemExporValoresNoHistorico()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var (administrador, igreja) = await CriarAdministradorEIgrejaAsync(aplicacao.Services);
        Guid atendimentoId;

        await using (var escopo = aplicacao.Services.CreateAsyncScope())
        {
            var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
            var atendimento = Atendimento.Criar(
                igreja.Id,
                "Visitante com dados internos",
                "+5555999998877",
                true,
                false,
                "teste-1",
                DateTimeOffset.UtcNow);
            banco.Atendimentos.Add(atendimento);
            await banco.SaveChangesAsync();
            atendimentoId = atendimento.Id;
        }

        var cliente = CriarCliente(aplicacao);
        await EntrarAsync(cliente, administrador.Email!, SenhaAdministrador);
        var resposta = await EnviarComCsrfAsync(
            cliente,
            HttpMethod.Put,
            $"/api/atendimentos/{atendimentoId}/dados-complementares?igrejaId={igreja.Id}",
            new
            {
                logradouro = "  Rua Esperança  ",
                numeroResidencia = "42-B",
                bairro = "Centro",
                complemento = "Fundos",
                dataNascimento = "1990-05-10",
                situacaoCongregacional = "CONGREGA",
                nomeIgrejaCongrega = "Igreja da Comunidade"
            });
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);

        await using var escopoValidacao = aplicacao.Services.CreateAsyncScope();
        var bancoValidacao = escopoValidacao.ServiceProvider.GetRequiredService<BancoContexto>();
        var atendimentoSalvo = await bancoValidacao.Atendimentos.SingleAsync(x => x.Id == atendimentoId);
        Assert.Equal("Rua Esperança", atendimentoSalvo.LogradouroVisitante);
        Assert.Equal("42-B", atendimentoSalvo.NumeroResidencia);
        Assert.Equal(new DateOnly(1990, 5, 10), atendimentoSalvo.DataNascimento);
        Assert.Equal(SituacaoCongregacional.Congrega, atendimentoSalvo.SituacaoCongregacional);
        Assert.Equal("Igreja da Comunidade", atendimentoSalvo.NomeIgrejaCongrega);

        var historico = await bancoValidacao.HistoricosAtendimento
            .SingleAsync(x => x.AtendimentoId == atendimentoId && x.Tipo == "DADOS_COMPLEMENTARES");
        Assert.Equal("Dados complementares do visitante atualizados.", historico.Descricao);
        Assert.DoesNotContain("Rua Esperança", historico.Descricao, StringComparison.Ordinal);
        Assert.DoesNotContain("Igreja da Comunidade", historico.Descricao, StringComparison.Ordinal);
    }

    private static async Task<(Usuario Administrador, Igreja Igreja)> CriarAdministradorEIgrejaAsync(
        IServiceProvider services)
    {
        var administrador = await CriarAdministradorAsync(services);
        await using var escopo = services.CreateAsyncScope();
        var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
        var igreja = CriarIgreja();
        banco.Igrejas.Add(igreja);
        await banco.SaveChangesAsync();
        return (administrador, igreja);
    }

    private static async Task<Usuario> CriarAdministradorAsync(IServiceProvider services) =>
        await CriarUsuarioAsync(
            services,
            "Administrador",
            "admin@teste.local",
            SenhaAdministrador,
            PerfilUsuario.Administrador,
            null,
            deveTrocarSenha: false);

    private static async Task<Usuario> CriarUsuarioAsync(
        IServiceProvider services,
        string nome,
        string email,
        string senha,
        PerfilUsuario perfil,
        Guid? igrejaId,
        bool deveTrocarSenha)
    {
        await using var escopo = services.CreateAsyncScope();
        var gerenciador = escopo.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var agora = DateTimeOffset.UtcNow;
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            LockoutEnabled = true,
            Perfil = perfil,
            IgrejaId = igrejaId,
            Ativo = true,
            DeveTrocarSenha = deveTrocarSenha,
            CriadoEm = agora,
            AtualizadoEm = agora
        };
        var resultado = await gerenciador.CreateAsync(usuario, senha);
        Assert.True(resultado.Succeeded, string.Join("; ", resultado.Errors.Select(x => x.Description)));
        return usuario;
    }

    private static Igreja CriarIgreja(string nome = "Igreja de teste") => Igreja.Criar(
        nome,
        "Rua das Flores",
        "123",
        "Centro",
        null,
        "98910000",
        "Três de Maio",
        "RS",
        DateTimeOffset.UtcNow);

    private static async Task<HttpResponseMessage> EntrarAsync(
        HttpClient cliente,
        string email,
        string senha,
        bool validarSucesso = true)
    {
        var resposta = await EnviarComCsrfAsync(
            cliente,
            HttpMethod.Post,
            "/api/autenticacao/entrar",
            new { email, senha });
        if (validarSucesso)
        {
            Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        }

        return resposta;
    }

    private static HttpClient CriarCliente(AplicacaoDeTeste aplicacao) =>
        aplicacao.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    private static async Task<HttpResponseMessage> EnviarComCsrfAsync(
        HttpClient cliente,
        HttpMethod metodo,
        string endereco,
        object corpo)
    {
        var token = await cliente.GetFromJsonAsync<TokenCsrf>("/api/seguranca/token-csrf");
        using var mensagem = new HttpRequestMessage(metodo, endereco)
        {
            Content = JsonContent.Create(corpo)
        };
        mensagem.Headers.Add("X-CSRF-TOKEN", token!.Token);
        return await cliente.SendAsync(mensagem);
    }

    private sealed record TokenCsrf(string Token);
    private sealed record SessaoResposta(bool DeveTrocarSenha);

    private sealed class AplicacaoDeTeste : WebApplicationFactory<Program>
    {
        private readonly string _nomeBanco = $"crm-ictm-gestao-{Guid.NewGuid()}";

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
            });
        }
    }
}
