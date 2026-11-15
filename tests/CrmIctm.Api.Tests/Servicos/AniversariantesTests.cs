using System.Globalization;
using CrmIctm.Api.Servicos;

namespace CrmIctm.Api.Tests.Servicos;

public sealed class AniversariantesTests
{
    [Theory]
    // 22h de 15/11 em Brasília já é 16/11 em UTC. A data tem de ser a de cá.
    [InlineData("2026-11-16T01:00:00Z", 2026, 11, 15)]
    [InlineData("2026-11-15T02:30:00Z", 2026, 11, 14)]
    [InlineData("2026-11-15T12:00:00Z", 2026, 11, 15)]
    public void HojeSegueOFusoDeBrasilia(string instante, int ano, int mes, int dia)
    {
        var hoje = Aniversariantes.Hoje(DateTimeOffset.Parse(instante, CultureInfo.InvariantCulture));

        Assert.Equal(new DateOnly(ano, mes, dia), hoje);
    }

    [Theory]
    [InlineData("2026-11-15", "2026-11-15")] // domingo
    [InlineData("2026-11-18", "2026-11-15")] // quarta
    [InlineData("2026-11-21", "2026-11-15")] // sábado
    public void SemanaComecaNoDomingo(string hoje, string domingoEsperado)
    {
        var domingo = Aniversariantes.InicioDaSemana(DateOnly.Parse(hoje, CultureInfo.InvariantCulture));

        Assert.Equal(DateOnly.Parse(domingoEsperado, CultureInfo.InvariantCulture), domingo);
    }

    [Fact]
    public void PeriodoDeHojeTemUmDia()
    {
        var datas = Aniversariantes.DatasDoPeriodo(
            PeriodoAniversario.Hoje, new DateOnly(2026, 11, 15));

        Assert.Equal([new DateOnly(2026, 11, 15)], datas);
    }

    [Fact]
    public void SemanaQueAtravessaOAnoMantemAOrdemDoCalendario()
    {
        var datas = Aniversariantes.DatasDoPeriodo(
            PeriodoAniversario.Semana, new DateOnly(2026, 12, 31));

        Assert.Equal(7, datas.Count);
        Assert.Equal(new DateOnly(2026, 12, 27), datas[0]);
        Assert.Equal(new DateOnly(2027, 1, 2), datas[^1]);
        // É isto que a ordenação por data preserva e a ordenação por mês e dia
        // perderia: 31 de dezembro vem antes de 1º de janeiro.
        Assert.True(datas.SequenceEqual(datas.OrderBy(x => x)));
    }

    [Fact]
    public void MesCobreOMesInteiro()
    {
        var datas = Aniversariantes.DatasDoPeriodo(
            PeriodoAniversario.Mes, new DateOnly(2026, 11, 15));

        Assert.Equal(30, datas.Count);
        Assert.Equal(new DateOnly(2026, 11, 1), datas[0]);
        Assert.Equal(new DateOnly(2026, 11, 30), datas[^1]);
    }

    [Fact]
    public void EmAnoComumNascidoEm29DeFevereiroCaiNoDia28()
    {
        var datas = Aniversariantes.DatasDoPeriodo(
            PeriodoAniversario.Hoje, new DateOnly(2027, 2, 28));
        var porChave = Aniversariantes.DatasPorChave(datas);

        Assert.Equal(new DateOnly(2027, 2, 28), porChave[Aniversariantes.Chave(2, 29)]);
    }

    [Fact]
    public void EmAnoBissextoNascidoEm29DeFevereiroCaiNoProprioDia()
    {
        var datas = Aniversariantes.DatasDoPeriodo(
            PeriodoAniversario.Hoje, new DateOnly(2028, 2, 29));
        var porChave = Aniversariantes.DatasPorChave(datas);

        Assert.Equal(new DateOnly(2028, 2, 29), porChave[Aniversariantes.Chave(2, 29)]);
        Assert.Single(porChave);
    }

    [Fact]
    public void FevereiroDeAnoComumInclui29NaListaDoMes()
    {
        var datas = Aniversariantes.DatasDoPeriodo(
            PeriodoAniversario.Mes, new DateOnly(2027, 2, 10));
        var porChave = Aniversariantes.DatasPorChave(datas);

        Assert.Equal(28, datas.Count);
        Assert.True(porChave.ContainsKey(Aniversariantes.Chave(2, 29)));
        Assert.Equal(new DateOnly(2027, 2, 28), porChave[Aniversariantes.Chave(2, 29)]);
    }

    [Fact]
    public void ChaveReuneMesEDia()
    {
        Assert.Equal(1115, Aniversariantes.Chave(new DateOnly(1990, 11, 15)));
        Assert.Equal(101, Aniversariantes.Chave(new DateOnly(1990, 1, 1)));
        Assert.Equal(1231, Aniversariantes.Chave(12, 31));
    }

    [Theory]
    [InlineData("1990-11-15", "2026-11-15", 36)]
    [InlineData("2000-02-29", "2027-02-28", 27)]
    [InlineData("2027-01-01", "2027-01-01", 0)]
    public void IdadeEhADaDataDoAniversarioENaoADeHoje(
        string nascimento, string ocorrencia, int esperada)
    {
        var idade = Aniversariantes.IdadeQueCompleta(
            DateOnly.Parse(nascimento, CultureInfo.InvariantCulture), DateOnly.Parse(ocorrencia, CultureInfo.InvariantCulture));

        Assert.Equal(esperada, idade);
    }

    [Fact]
    public void AdultoFalaPeloProprioNumero()
    {
        var (destino, whatsapp) = Aniversariantes.Destino(
            ehMenor: false, "+5541998124455", null);

        Assert.Equal(DestinoAniversario.Membro, destino);
        Assert.Equal("+5541998124455", whatsapp);
    }

    [Fact]
    public void AdultoSemWhatsappNaoTemDestino()
    {
        var (destino, whatsapp) = Aniversariantes.Destino(
            ehMenor: false, null, "+5541996541120");

        Assert.Equal(DestinoAniversario.Nenhum, destino);
        Assert.Null(whatsapp);
    }

    [Fact]
    public void MenorComNumeroProprioFalaPeloProprioNumero()
    {
        var (destino, whatsapp) = Aniversariantes.Destino(
            ehMenor: true, "+5541991112222", "+5541996541120");

        Assert.Equal(DestinoAniversario.Membro, destino);
        Assert.Equal("+5541991112222", whatsapp);
    }

    [Fact]
    public void MenorSemNumeroProprioCaiNoResponsavel()
    {
        var (destino, whatsapp) = Aniversariantes.Destino(
            ehMenor: true, null, "+5541996541120");

        Assert.Equal(DestinoAniversario.Responsavel, destino);
        Assert.Equal("+5541996541120", whatsapp);
    }

    [Fact]
    public void MenorSemNenhumNumeroNaoTemDestino()
    {
        var (destino, whatsapp) = Aniversariantes.Destino(ehMenor: true, "   ", null);

        Assert.Equal(DestinoAniversario.Nenhum, destino);
        Assert.Null(whatsapp);
    }

    [Theory]
    [InlineData(null, PeriodoAniversario.Hoje)]
    [InlineData("", PeriodoAniversario.Hoje)]
    [InlineData("HOJE", PeriodoAniversario.Hoje)]
    [InlineData("semana", PeriodoAniversario.Semana)]
    [InlineData(" Mes ", PeriodoAniversario.Mes)]
    public void PeriodoEhLidoSemDistinguirCaixa(string? codigo, PeriodoAniversario esperado)
    {
        var lido = Aniversariantes.TentarLerPeriodo(codigo, out var periodo);

        Assert.True(lido);
        Assert.Equal(esperado, periodo);
    }

    [Theory]
    [InlineData("ANO")]
    [InlineData("mês")]
    [InlineData("tudo")]
    public void PeriodoDesconhecidoEhRecusado(string codigo)
    {
        Assert.False(Aniversariantes.TentarLerPeriodo(codigo, out _));
    }
}
