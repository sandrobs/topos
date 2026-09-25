namespace CrmIctm.Api.Dominio;

public sealed class Atendimento
{
    private Atendimento()
    {
    }

    public Guid Id { get; private set; }
    public Guid IgrejaId { get; private set; }
    public Igreja Igreja { get; private set; } = null!;
    public string NomeVisitante { get; private set; } = string.Empty;
    public string Whatsapp { get; private set; } = string.Empty;
    public bool QuerConhecerIgreja { get; private set; }
    public bool QuerConversaOracao { get; private set; }
    public EtapaAtendimento Etapa { get; private set; }
    public Guid? ResponsavelId { get; private set; }
    public Usuario? Responsavel { get; private set; }
    public string AvisoPrivacidadeVersao { get; private set; } = string.Empty;
    public string? LogradouroVisitante { get; private set; }
    public string? NumeroResidencia { get; private set; }
    public string? BairroVisitante { get; private set; }
    public string? ComplementoEndereco { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public SituacaoCongregacional SituacaoCongregacional { get; private set; }
    public string? NomeIgrejaCongrega { get; private set; }
    public ResultadoAtendimento? Resultado { get; private set; }
    public DateTimeOffset? ConcluidoEm { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset UltimaAtividadeEm { get; private set; }

    public static Atendimento Criar(
        Guid igrejaId,
        string nomeVisitante,
        string whatsapp,
        bool querConhecerIgreja,
        bool querConversaOracao,
        string avisoPrivacidadeVersao,
        DateTimeOffset agora)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nomeVisitante);
        ArgumentException.ThrowIfNullOrWhiteSpace(whatsapp);
        ArgumentException.ThrowIfNullOrWhiteSpace(avisoPrivacidadeVersao);

        if (!querConhecerIgreja && !querConversaOracao)
        {
            throw new ArgumentException("Ao menos um interesse deve ser informado.");
        }

        return new Atendimento
        {
            Id = Guid.NewGuid(),
            IgrejaId = igrejaId,
            NomeVisitante = nomeVisitante.Trim(),
            Whatsapp = whatsapp,
            QuerConhecerIgreja = querConhecerIgreja,
            QuerConversaOracao = querConversaOracao,
            Etapa = EtapaAtendimento.Novo,
            SituacaoCongregacional = SituacaoCongregacional.NaoInformado,
            AvisoPrivacidadeVersao = avisoPrivacidadeVersao,
            CriadoEm = agora,
            UltimaAtividadeEm = agora
        };
    }

    public void AlterarEtapa(
        EtapaAtendimento novaEtapa,
        ResultadoAtendimento? novoResultado,
        DateTimeOffset agora)
    {
        if (Math.Abs((int)novaEtapa - (int)Etapa) != 1)
        {
            throw new ArgumentException(
                "O atendimento só pode ser movido para a etapa imediatamente anterior ou posterior.");
        }

        if (novaEtapa == EtapaAtendimento.Concluido && novoResultado is null)
        {
            throw new ArgumentException("Um resultado é obrigatório para concluir o atendimento.");
        }

        Etapa = novaEtapa;
        Resultado = novaEtapa == EtapaAtendimento.Concluido ? novoResultado : null;
        ConcluidoEm = novaEtapa == EtapaAtendimento.Concluido ? agora : null;
        UltimaAtividadeEm = agora;
    }

    public void AtribuirResponsavel(Guid? responsavelId, DateTimeOffset agora)
    {
        ResponsavelId = responsavelId;
        UltimaAtividadeEm = agora;
    }

    public void RegistrarAtividade(DateTimeOffset agora)
    {
        UltimaAtividadeEm = agora;
    }

    public void AtualizarDadosComplementares(
        string? logradouro,
        string? numeroResidencia,
        string? bairro,
        string? complemento,
        DateOnly? dataNascimento,
        SituacaoCongregacional situacaoCongregacional,
        string? nomeIgrejaCongrega,
        DateTimeOffset agora)
    {
        var hoje = DateOnly.FromDateTime(agora.LocalDateTime);
        if (dataNascimento.HasValue && dataNascimento.Value > hoje)
        {
            throw new ArgumentException("A data de nascimento não pode estar no futuro.");
        }

        LogradouroVisitante = NormalizarOpcional(logradouro);
        NumeroResidencia = NormalizarOpcional(numeroResidencia);
        BairroVisitante = NormalizarOpcional(bairro);
        ComplementoEndereco = NormalizarOpcional(complemento);
        DataNascimento = dataNascimento;
        SituacaoCongregacional = situacaoCongregacional;
        NomeIgrejaCongrega = situacaoCongregacional == SituacaoCongregacional.Congrega
            ? NormalizarOpcional(nomeIgrejaCongrega)
            : null;
        UltimaAtividadeEm = agora;
    }

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
