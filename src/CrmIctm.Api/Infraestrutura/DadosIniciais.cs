using CrmIctm.Api.Configuracao;
using CrmIctm.Api.Dominio;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CrmIctm.Api.Infraestrutura;

public static class DadosIniciais
{
    private static readonly Action<ILogger, string, Exception?> RegistrarIgrejaInicialCriada =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1001, "IgrejaInicialCriada"),
            "Igreja inicial criada. Endereço público: /visita/{IdentificadorPublico}");

    public static async Task PrepararAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        await using var escopo = app.Services.CreateAsyncScope();
        var configuracao = escopo.ServiceProvider.GetRequiredService<IConfiguration>();
        var banco = escopo.ServiceProvider.GetRequiredService<BancoContexto>();

        if (configuracao.GetValue<bool>("Banco:AplicarMigracoesAoIniciar"))
        {
            await banco.Database.MigrateAsync(cancellationToken);
        }

        await CriarIgrejaInicialAsync(escopo.ServiceProvider, banco, cancellationToken);

        var opcoes = escopo.ServiceProvider
            .GetRequiredService<IOptions<DadosIniciaisOpcoes>>()
            .Value;

        if (!opcoes.EstaConfigurado)
        {
            return;
        }

        var gerenciador = escopo.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        if (await gerenciador.FindByEmailAsync(opcoes.Email) is not null)
        {
            return;
        }

        var administrador = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = opcoes.Nome.Trim(),
            UserName = opcoes.Email.Trim(),
            Email = opcoes.Email.Trim(),
            EmailConfirmed = true,
            LockoutEnabled = true,
            Perfil = PerfilUsuario.Administrador,
            Ativo = true,
            DeveTrocarSenha = false,
            CriadoEm = DateTimeOffset.UtcNow,
            AtualizadoEm = DateTimeOffset.UtcNow
        };

        var resultado = await gerenciador.CreateAsync(administrador, opcoes.Senha);
        if (!resultado.Succeeded)
        {
            var erros = string.Join("; ", resultado.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"Não foi possível criar o administrador inicial: {erros}");
        }
    }

    private static async Task CriarIgrejaInicialAsync(
        IServiceProvider servicos,
        BancoContexto banco,
        CancellationToken cancellationToken)
    {
        var opcoes = servicos.GetRequiredService<IOptions<IgrejaInicialOpcoes>>().Value;
        if (!opcoes.EstaConfigurado)
        {
            return;
        }

        var nome = opcoes.Nome.Trim();
        var cidade = opcoes.Cidade.Trim();
        var estado = opcoes.Estado.Trim().ToUpperInvariant();
        var jaExiste = await banco.Igrejas.AnyAsync(
            x => x.Nome == nome && x.Cidade == cidade && x.Estado == estado,
            cancellationToken);

        if (jaExiste)
        {
            return;
        }

        var igreja = Igreja.Criar(
            nome,
            opcoes.Logradouro,
            opcoes.Numero,
            opcoes.Bairro,
            opcoes.Complemento,
            opcoes.Cep,
            cidade,
            estado,
            DateTimeOffset.UtcNow);
        banco.Igrejas.Add(igreja);
        await banco.SaveChangesAsync(cancellationToken);

        var logger = servicos.GetRequiredService<ILoggerFactory>().CreateLogger("DadosIniciais");
        RegistrarIgrejaInicialCriada(logger, igreja.IdentificadorPublico, null);
    }
}
