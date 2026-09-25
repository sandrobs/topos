using System.Net;
using System.Net.Http.Json;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Infraestrutura;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CrmIctm.Api.Tests.Integracao;

public sealed class FormularioPublicoTests
{
    [Fact]
    public async Task DeveCriarAtendimentoNaIgrejaIdentificadaPelaUrl()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var igreja = await CriarIgrejaAsync(aplicacao.Services);
        var cliente = aplicacao.CreateClient();

        var resposta = await cliente.PostAsJsonAsync(
            $"/api/publico/igrejas/{igreja.IdentificadorPublico}/atendimentos",
            new
            {
                nome = "Maria da Silva",
                whatsapp = "(11) 98765-4321",
                querConhecerIgreja = true,
                querConversaOracao = false,
                avisoPrivacidadeReconhecido = true,
                avisoPrivacidadeVersao = "teste-1"
            });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        await using var escopo = aplicacao.Services.CreateAsyncScope();
        var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
        var atendimento = await banco.Atendimentos.SingleAsync();
        Assert.Equal(igreja.Id, atendimento.IgrejaId);
        Assert.Equal("+5511987654321", atendimento.Whatsapp);
        Assert.Equal(EtapaAtendimento.Novo, atendimento.Etapa);
        Assert.Equal("teste-1", atendimento.AvisoPrivacidadeVersao);
    }

    [Fact]
    public async Task DeveRecusarIgrejaInativaComMensagemNeutra()
    {
        await using var aplicacao = new AplicacaoDeTeste();
        var cliente = aplicacao.CreateClient();

        var resposta = await cliente.PostAsJsonAsync(
            "/api/publico/igrejas/identificador-inexistente/atendimentos",
            new
            {
                nome = "Maria",
                whatsapp = "11987654321",
                querConhecerIgreja = true,
                querConversaOracao = false,
                avisoPrivacidadeReconhecido = true,
                avisoPrivacidadeVersao = "teste-1"
            });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        var conteudo = await resposta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("banco", conteudo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("inativa", conteudo, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<Igreja> CriarIgrejaAsync(IServiceProvider services)
    {
        await using var escopo = services.CreateAsyncScope();
        var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();
        var igreja = Igreja.Criar(
            "Igreja de teste",
            "Rua das Flores",
            "123",
            "Centro",
            null,
            "01001000",
            "São Paulo",
            "SP",
            DateTimeOffset.UtcNow);
        banco.Igrejas.Add(igreja);
        await banco.SaveChangesAsync();
        return igreja;
    }

    private sealed class AplicacaoDeTeste : WebApplicationFactory<Program>
    {
        private readonly string _nomeBanco = $"crm-ictm-testes-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration((_, configuracao) =>
            {
                configuracao.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AvisoPrivacidade:Versao"] = "teste-1",
                    ["AvisoPrivacidade:Texto"] = "Aviso usado exclusivamente pelos testes automatizados."
                });
            });
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
