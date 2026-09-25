namespace CrmIctm.Api.Configuracao;

public sealed class AvisoPrivacidadeMembrosOpcoes
{
    public const string Secao = "AvisoPrivacidadeMembros";

    public string Versao { get; init; } = string.Empty;
    public string Texto { get; init; } = string.Empty;
    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(Versao) && !string.IsNullOrWhiteSpace(Texto);
}
