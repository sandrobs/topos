namespace CrmIctm.Api.Configuracao;

public sealed class IgrejaInicialOpcoes
{
    public const string Secao = "DadosIniciais:Igreja";

    public string Nome { get; init; } = string.Empty;
    public string Logradouro { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string Bairro { get; init; } = string.Empty;
    public string? Complemento { get; init; }
    public string Cep { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;

    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(Nome) &&
        !string.IsNullOrWhiteSpace(Logradouro) &&
        !string.IsNullOrWhiteSpace(Numero) &&
        !string.IsNullOrWhiteSpace(Bairro) &&
        Cep.Count(char.IsDigit) == 8 &&
        !string.IsNullOrWhiteSpace(Cidade) &&
        Estado.Trim().Length == 2;
}
