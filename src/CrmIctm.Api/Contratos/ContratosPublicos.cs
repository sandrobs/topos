namespace CrmIctm.Api.Contratos;

public sealed record NovoAtendimentoPublicoRequisicao(
    string? Nome,
    string? Whatsapp,
    bool QuerConhecerIgreja,
    bool QuerConversaOracao,
    bool AvisoPrivacidadeReconhecido,
    string? AvisoPrivacidadeVersao);

public sealed record EntrarRequisicao(string? Email, string? Senha);

public sealed record AlterarSenhaRequisicao(
    string? SenhaAtual,
    string? NovaSenha,
    string? ConfirmacaoSenha);

public sealed record SalvarIgrejaRequisicao(
    string? Nome,
    string? Logradouro,
    string? Numero,
    string? Bairro,
    string? Complemento,
    string? Cep,
    string? Cidade,
    string? Estado);

public sealed record AlterarSituacaoRequisicao(bool Ativo);

public sealed record CriarUsuarioRequisicao(
    string? Nome,
    string? Email,
    string? Perfil,
    Guid? IgrejaId,
    string? SenhaTemporaria);

public sealed record EditarUsuarioRequisicao(string? Nome, string? Email, string? Perfil);

public sealed record RedefinirSenhaRequisicao(string? SenhaTemporaria);

public sealed record AdicionarObservacaoRequisicao(string? Texto);

public sealed record AlterarEtapaRequisicao(string? Etapa, string? Resultado);

public sealed record AtribuirResponsavelRequisicao(Guid? ResponsavelId);

public sealed record AtualizarDadosComplementaresRequisicao(
    string? Logradouro,
    string? NumeroResidencia,
    string? Bairro,
    string? Complemento,
    DateOnly? DataNascimento,
    string? SituacaoCongregacional,
    string? NomeIgrejaCongrega);
