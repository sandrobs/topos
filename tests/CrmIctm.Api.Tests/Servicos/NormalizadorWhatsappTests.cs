using CrmIctm.Api.Servicos;

namespace CrmIctm.Api.Tests.Servicos;

public sealed class NormalizadorWhatsappTests
{
    [Theory]
    [InlineData("(11) 98765-4321", "+5511987654321")]
    [InlineData("55 11 98765-4321", "+5511987654321")]
    [InlineData("+55 (21) 3456-7890", "+552134567890")]
    public void DeveNormalizarNumeroBrasileiro(string informado, string esperado)
    {
        var valido = NormalizadorWhatsapp.TentarNormalizar(informado, out var normalizado);

        Assert.True(valido);
        Assert.Equal(esperado, normalizado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("00 99999-9999")]
    [InlineData("11 12345-6789")]
    public void DeveRecusarNumeroInvalido(string? informado)
    {
        var valido = NormalizadorWhatsapp.TentarNormalizar(informado, out var normalizado);

        Assert.False(valido);
        Assert.Empty(normalizado);
    }
}

