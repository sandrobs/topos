namespace CrmIctm.Api.Dominio;

public sealed class RegistroAuditoriaAdministrativa
{
    private RegistroAuditoriaAdministrativa()
    {
    }

    public Guid Id { get; private set; }
    public string TipoEntidade { get; private set; } = string.Empty;
    public Guid EntidadeId { get; private set; }
    public string Acao { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public Guid AutorId { get; private set; }
    public Usuario Autor { get; private set; } = null!;
    public DateTimeOffset CriadoEm { get; private set; }

    public static RegistroAuditoriaAdministrativa Criar(
        string tipoEntidade,
        Guid entidadeId,
        string acao,
        string descricao,
        Guid autorId,
        DateTimeOffset agora)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tipoEntidade);
        ArgumentException.ThrowIfNullOrWhiteSpace(acao);
        ArgumentException.ThrowIfNullOrWhiteSpace(descricao);

        return new RegistroAuditoriaAdministrativa
        {
            Id = Guid.NewGuid(),
            TipoEntidade = tipoEntidade.Trim(),
            EntidadeId = entidadeId,
            Acao = acao.Trim(),
            Descricao = descricao.Trim(),
            AutorId = autorId,
            CriadoEm = agora
        };
    }
}
