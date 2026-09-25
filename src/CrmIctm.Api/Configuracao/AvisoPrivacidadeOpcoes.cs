namespace CrmIctm.Api.Configuracao;

public sealed class AvisoPrivacidadeOpcoes
{
    public const string Secao = "AvisoPrivacidade";

    public string Versao { get; init; } = string.Empty;
    public string Texto { get; init; } = string.Empty;

    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(Versao) && !string.IsNullOrWhiteSpace(Texto);
}

