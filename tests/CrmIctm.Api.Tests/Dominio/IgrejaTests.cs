using CrmIctm.Api.Dominio;

namespace CrmIctm.Api.Tests.Dominio;

public sealed class IgrejaTests
{
    [Fact]
    public void DeveCriarIdentificadorPublicoAleatorioEEstavel()
    {
        var agora = DateTimeOffset.UtcNow;
        var primeira = CriarIgreja("Igreja A", "sp", agora);
        var segunda = CriarIgreja("Igreja B", "SP", agora);

        Assert.NotEmpty(primeira.IdentificadorPublico);
        Assert.NotEqual(primeira.IdentificadorPublico, segunda.IdentificadorPublico);
        Assert.Equal("SP", primeira.Estado);
        Assert.True(primeira.Ativa);
    }

    [Fact]
    public void DeveAtualizarEnderecoSemAlterarIdentificadorPublico()
    {
        var igreja = CriarIgreja("Igreja A", "RS", DateTimeOffset.UtcNow);
        var identificador = igreja.IdentificadorPublico;
        var atualizadoEm = DateTimeOffset.UtcNow.AddMinutes(1);

        igreja.AtualizarDados(
            "Igreja Renovada",
            "Avenida Central",
            "S/N",
            "Centro",
            "Sala 2",
            "99999-000",
            "Três de Maio",
            "RS",
            atualizadoEm);

        Assert.Equal(identificador, igreja.IdentificadorPublico);
        Assert.Equal("99999000", igreja.Cep);
        Assert.Equal("Sala 2", igreja.Complemento);
        Assert.Equal(atualizadoEm, igreja.AtualizadaEm);
    }

    [Fact]
    public void DeveAlterarSituacaoSemApagarCadastro()
    {
        var igreja = CriarIgreja("Igreja A", "RS", DateTimeOffset.UtcNow);

        igreja.AlterarSituacao(false, DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.False(igreja.Ativa);
        Assert.NotEmpty(igreja.IdentificadorPublico);
    }

    private static Igreja CriarIgreja(string nome, string estado, DateTimeOffset agora) =>
        Igreja.Criar(
            nome,
            "Rua das Flores",
            "123",
            "Centro",
            null,
            "98910000",
            "Três de Maio",
            estado,
            agora);
}
