namespace CrmIctm.Api.Dominio;

public static class UnidadesFederativas
{
    private static readonly HashSet<string> Codigos =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
        "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
        "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    public static bool EhValida(string? estado) =>
        !string.IsNullOrWhiteSpace(estado) && Codigos.Contains(estado.Trim().ToUpperInvariant());
}
