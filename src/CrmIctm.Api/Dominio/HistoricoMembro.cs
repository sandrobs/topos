namespace CrmIctm.Api.Dominio;

public sealed class HistoricoMembro
{
    private HistoricoMembro() { }

    public Guid Id { get; private set; }
    public Guid IgrejaId { get; private set; }
    public Guid? MembroId { get; private set; }
    public Guid? SolicitacaoId { get; private set; }
    public Guid AutorId { get; private set; }
    public Usuario Autor { get; private set; } = null!;
    public string Acao { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }

    public static HistoricoMembro Criar(Guid igrejaId, Guid? membroId,
        Guid? solicitacaoId, Guid autorId, string acao,
        string? descricao, DateTimeOffset agora) => new()
        {
            Id = Guid.NewGuid(), IgrejaId = igrejaId, MembroId = membroId,
            SolicitacaoId = solicitacaoId, AutorId = autorId,
            Acao = acao, Descricao = descricao, CriadoEm = agora
        };
}
