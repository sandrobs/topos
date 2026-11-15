namespace CrmIctm.Api.Dominio;

public sealed class SolicitacaoMembro
{
    private SolicitacaoMembro() { }

    public Guid Id { get; private set; }
    public Guid IgrejaId { get; private set; }
    public Igreja Igreja { get; private set; } = null!;
    public DadosCadastraisMembro Dados { get; private set; } = null!;
    public string Estado { get; private set; } = "PENDENTE";
    public string AvisoPrivacidadeVersao { get; private set; } = string.Empty;
    public DateTimeOffset CriadaEm { get; private set; }
    public DateTimeOffset AtualizadaEm { get; private set; }
    public Guid? DecididaPorId { get; private set; }
    public Usuario? DecididaPor { get; private set; }
    public DateTimeOffset? DecididaEm { get; private set; }
    public string? MotivoRecusa { get; private set; }
    public Guid? MembroId { get; private set; }

    public static SolicitacaoMembro Criar(
        Guid igrejaId, DadosCadastraisMembro dados, string versaoAviso,
        DateTimeOffset agora) => new()
        {
            Id = Guid.NewGuid(), IgrejaId = igrejaId, Dados = dados.Copiar(),
            AvisoPrivacidadeVersao = versaoAviso, CriadaEm = agora, AtualizadaEm = agora
        };

    public void Corrigir(DadosCadastraisMembro dados, DateTimeOffset agora)
    {
        if (Estado != "PENDENTE") throw new InvalidOperationException("A solicitação já foi decidida.");
        Dados = dados.Copiar();
        AtualizadaEm = agora;
    }

    public void Aprovar(Guid autorId, Guid membroId, DateTimeOffset agora)
    {
        if (Estado != "PENDENTE") throw new InvalidOperationException("A solicitação já foi decidida.");
        Estado = "APROVADA";
        MembroId = membroId;
        DecididaPorId = autorId;
        DecididaEm = agora;
        AtualizadaEm = agora;
    }

    public void Recusar(Guid autorId, string motivo, DateTimeOffset agora)
    {
        if (Estado != "PENDENTE") throw new InvalidOperationException("A solicitação já foi decidida.");
        Estado = "RECUSADA";
        MotivoRecusa = motivo.Trim();
        DecididaPorId = autorId;
        DecididaEm = agora;
        AtualizadaEm = agora;
    }
}
