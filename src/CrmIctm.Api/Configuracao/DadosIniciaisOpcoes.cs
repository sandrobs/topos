namespace CrmIctm.Api.Configuracao;

public sealed class DadosIniciaisOpcoes
{
    public const string Secao = "DadosIniciais:Administrador";

    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;

    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(Nome) &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Senha);
}

