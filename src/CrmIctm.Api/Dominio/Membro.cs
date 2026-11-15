namespace CrmIctm.Api.Dominio;

public sealed class Membro
{
    private Membro() { }

    public Guid Id { get; private set; }
    public Guid IgrejaId { get; private set; }
    public Igreja Igreja { get; private set; } = null!;
    public DadosCadastraisMembro Dados { get; private set; } = null!;
    public string Situacao { get; private set; } = "ATIVO";
    public string Origem { get; private set; } = string.Empty;
    public DateOnly? DataIngresso { get; private set; }
    public string? ObservacaoPastoral { get; private set; }
    public string? MotivoInativacao { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset AtualizadoEm { get; private set; }

    public static Membro Criar(Guid igrejaId, DadosCadastraisMembro dados,
        string origem, DateOnly? dataIngresso, string? observacaoPastoral,
        DateTimeOffset agora) => new()
        {
            Id = Guid.NewGuid(), IgrejaId = igrejaId, Dados = dados.Copiar(),
            Origem = origem, DataIngresso = dataIngresso,
            ObservacaoPastoral = Limpar(observacaoPastoral),
            CriadoEm = agora, AtualizadoEm = agora
        };

    public void Atualizar(DadosCadastraisMembro dados, DateOnly? dataIngresso,
        string? observacaoPastoral, DateTimeOffset agora)
    {
        Dados = dados.Copiar();
        DataIngresso = dataIngresso;
        ObservacaoPastoral = Limpar(observacaoPastoral);
        AtualizadoEm = agora;
    }

    public void AlterarSituacao(bool ativo, string? motivo, DateTimeOffset agora)
    {
        Situacao = ativo ? "ATIVO" : "INATIVO";
        MotivoInativacao = ativo ? null : Limpar(motivo);
        AtualizadoEm = agora;
    }

    private static string? Limpar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
