using CrmIctm.Api.Contratos;
using CrmIctm.Api.Servicos;

namespace CrmIctm.Api.Tests.Servicos;

public sealed class ValidacaoDadosMembroTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("NAO_INFORMADO")]
    [InlineData("ADMINISTRADOR")]
    public void FuncaoAusenteOuInvalidaEhRecusada(string? funcao)
    {
        Assert.Contains("funcao", ValidacaoDadosMembro.Validar(Dados(funcao), true, new DateOnly(2026, 10, 3)));
    }

    [Theory]
    [InlineData("MEMBRO")]
    [InlineData("DIACONO")]
    [InlineData("PRESBITERO")]
    [InlineData("PASTOR")]
    public void FuncaoValidaEhPreservadaInclusiveNaCopiaParaAprovacao(string funcao)
    {
        var dados = Dados(funcao);
        Assert.Empty(ValidacaoDadosMembro.Validar(dados, true, new DateOnly(2026, 10, 3)));
        Assert.Equal(funcao, ValidacaoDadosMembro.Criar(dados).Copiar().Funcao);
    }

    private static DadosMembroRequisicao Dados(string? funcao) =>
        new("Maria", "Teste", "(55) 99999-1234", null, null, null, null,
            null, null, null, null, null, "NAO_INFORMADO", null, Funcao: funcao);
}
