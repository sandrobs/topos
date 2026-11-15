namespace CrmIctm.Api.Dominio;

// Informação eclesiástica. Não representa um perfil de acesso ao sistema.
public static class FuncoesMembro
{
    public static bool EhValida(string? funcao) =>
        funcao is "MEMBRO" or "DIACONO" or "PRESBITERO" or "PASTOR";
}
