namespace CrmIctm.Api.Dominio;

public sealed class HistoricoAtendimento
{
    public Guid Id { get; set; }
    public Guid AtendimentoId { get; set; }
    public Atendimento Atendimento { get; set; } = null!;
    public Guid AutorId { get; set; }
    public Usuario Autor { get; set; } = null!;
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; set; }
}
