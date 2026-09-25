using CrmIctm.Api.Dominio;

namespace CrmIctm.Api.Tests.Dominio;

public sealed class AtendimentoTests
{
    [Fact]
    public void NovoAtendimentoDeveComecarNaEtapaNovo()
    {
        var agora = new DateTimeOffset(2026, 9, 15, 20, 0, 0, TimeSpan.Zero);

        var atendimento = Atendimento.Criar(
            Guid.NewGuid(),
            "  Maria da Silva  ",
            "+5511987654321",
            querConhecerIgreja: true,
            querConversaOracao: false,
            "2026-09-01",
            agora);

        Assert.Equal("Maria da Silva", atendimento.NomeVisitante);
        Assert.Equal(EtapaAtendimento.Novo, atendimento.Etapa);
        Assert.Equal(agora, atendimento.CriadoEm);
        Assert.Equal(agora, atendimento.UltimaAtividadeEm);
        Assert.Null(atendimento.Resultado);
    }

    [Fact]
    public void DeveExigirAoMenosUmInteresse()
    {
        var excecao = Assert.Throws<ArgumentException>(() => Atendimento.Criar(
            Guid.NewGuid(),
            "Maria",
            "+5511987654321",
            querConhecerIgreja: false,
            querConversaOracao: false,
            "2026-09-01",
            DateTimeOffset.UtcNow));

        Assert.Contains("interesse", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeveExigirResultadoAoConcluir()
    {
        var atendimento = CriarAtendimento();
        AvancarAteEmAcompanhamento(atendimento);

        var excecao = Assert.Throws<ArgumentException>(() => atendimento.AlterarEtapa(
            EtapaAtendimento.Concluido,
            novoResultado: null,
            DateTimeOffset.UtcNow));

        Assert.Contains("resultado", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeveLimparConclusaoAtualAoReabrir()
    {
        var atendimento = CriarAtendimento();
        var conclusao = DateTimeOffset.UtcNow;
        AvancarAteEmAcompanhamento(atendimento);
        atendimento.AlterarEtapa(
            EtapaAtendimento.Concluido,
            ResultadoAtendimento.SemRetorno,
            conclusao);

        atendimento.AlterarEtapa(
            EtapaAtendimento.EmAcompanhamento,
            novoResultado: null,
            conclusao.AddDays(1));

        Assert.Equal(EtapaAtendimento.EmAcompanhamento, atendimento.Etapa);
        Assert.Null(atendimento.Resultado);
        Assert.Null(atendimento.ConcluidoEm);
    }

    [Theory]
    [InlineData(EtapaAtendimento.EmAtendimento)]
    [InlineData(EtapaAtendimento.Concluido)]
    public void NaoDevePermitirPularEtapasAoAvancar(EtapaAtendimento destino)
    {
        var atendimento = CriarAtendimento();

        var excecao = Assert.Throws<ArgumentException>(() => atendimento.AlterarEtapa(
            destino,
            destino == EtapaAtendimento.Concluido ? ResultadoAtendimento.SemRetorno : null,
            DateTimeOffset.UtcNow));

        Assert.Contains("imediatamente", excecao.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(EtapaAtendimento.Novo, atendimento.Etapa);
    }

    [Fact]
    public void NaoDevePermitirPularEtapasAoRetroceder()
    {
        var atendimento = CriarAtendimento();
        AvancarAteEmAcompanhamento(atendimento);

        var excecao = Assert.Throws<ArgumentException>(() => atendimento.AlterarEtapa(
            EtapaAtendimento.AguardandoContato,
            novoResultado: null,
            DateTimeOffset.UtcNow));

        Assert.Contains("imediatamente", excecao.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(EtapaAtendimento.EmAcompanhamento, atendimento.Etapa);
    }

    private static void AvancarAteEmAcompanhamento(Atendimento atendimento)
    {
        var agora = DateTimeOffset.UtcNow;
        atendimento.AlterarEtapa(EtapaAtendimento.AguardandoContato, null, agora);
        atendimento.AlterarEtapa(EtapaAtendimento.EmAtendimento, null, agora.AddMinutes(1));
        atendimento.AlterarEtapa(EtapaAtendimento.EmAcompanhamento, null, agora.AddMinutes(2));
    }

    private static Atendimento CriarAtendimento() => Atendimento.Criar(
        Guid.NewGuid(),
        "Maria",
        "+5511987654321",
        querConhecerIgreja: true,
        querConversaOracao: false,
        "2026-09-01",
        DateTimeOffset.UtcNow);
}
