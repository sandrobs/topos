namespace CrmIctm.Api.Contratos;

public sealed record DadosMembroRequisicao(
    string? Nome, string? Sobrenome, string? Whatsapp, string? Email,
    DateOnly? DataNascimento, string? Logradouro, string? Numero,
    string? Bairro, string? Complemento, string? Cep, string? Cidade,
    string? Estado, string? SituacaoBatismo, DateOnly? DataBatismo,
    bool EhMenor = false, string? NomeResponsavelLegal = null,
    string? WhatsappResponsavelLegal = null, string? VinculoResponsavelLegal = null);

public sealed record EnviarSolicitacaoMembroRequisicao(
    DadosMembroRequisicao? Dados, bool DeclaracaoMaioridade,
    bool AvisoPrivacidadeReconhecido, string? AvisoPrivacidadeVersao);

public sealed record SalvarMembroRequisicao(
    DadosMembroRequisicao? Dados, DateOnly? DataIngresso,
    string? ObservacaoPastoral, bool ConfirmarNovoApesarDuplicidade = false);

public sealed record CorrigirSolicitacaoMembroRequisicao(DadosMembroRequisicao? Dados);

public sealed record AprovarSolicitacaoMembroRequisicao(
    Guid? MembroExistenteId, bool ConfirmarNovoApesarDuplicidade,
    DateOnly? DataIngresso, string? ObservacaoPastoral);

public sealed record RecusarSolicitacaoMembroRequisicao(string? Motivo);

public sealed record AlterarSituacaoMembroRequisicao(bool Ativo, string? Motivo);
