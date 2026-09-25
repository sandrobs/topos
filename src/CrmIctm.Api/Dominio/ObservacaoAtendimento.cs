namespace CrmIctm.Api.Dominio;

public sealed class ObservacaoAtendimento
{
    public Guid Id { get; set; }
    public Guid AtendimentoId { get; set; }
    public Atendimento Atendimento { get; set; } = null!;
    public Guid AutorId { get; set; }
    public Usuario Autor { get; set; } = null!;
    public string Texto { get; set; } = string.Empty;
    public DateTimeOffset CriadaEm { get; set; }
}

