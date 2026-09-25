export type IgrejaPublica = {
  nome: string;
  logradouro: string;
  numero: string;
  bairro: string;
  complemento: string | null;
  cep: string;
  cidade: string;
  estado: string;
  avisoPrivacidade: {
    versao: string;
    texto: string;
  } | null;
};

export type NovoAtendimento = {
  nome: string;
  whatsapp: string;
  querConhecerIgreja: boolean;
  querConversaOracao: boolean;
  avisoPrivacidadeReconhecido: boolean;
  avisoPrivacidadeVersao: string;
};

export type ErrosValidacao = Record<string, string[]>;

export class ErroDaApi extends Error {
  public constructor(
    mensagem: string,
    public readonly erros: ErrosValidacao = {},
    public readonly status = 0,
    public readonly duplicados: Array<{ id: string; nome: string; sobrenome: string; situacao: string }> = [],
  ) {
    super(mensagem);
  }
}

export async function obterIgreja(identificador: string): Promise<IgrejaPublica> {
  const resposta = await fetch(`/api/publico/igrejas/${encodeURIComponent(identificador)}`);
  if (!resposta.ok) {
    throw new ErroDaApi("Esta página não está disponível.", {}, resposta.status);
  }

  return (await resposta.json()) as IgrejaPublica;
}

export async function enviarAtendimento(
  identificador: string,
  atendimento: NovoAtendimento,
): Promise<string> {
  const resposta = await fetch(
    `/api/publico/igrejas/${encodeURIComponent(identificador)}/atendimentos`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(atendimento),
    },
  );

  const conteudo = (await resposta.json().catch(() => ({}))) as {
    mensagem?: string;
    detail?: string;
    errors?: ErrosValidacao;
  };

  if (!resposta.ok) {
    throw new ErroDaApi(
      conteudo.detail ?? "Não foi possível enviar seus dados. Tente novamente.",
      conteudo.errors,
      resposta.status,
    );
  }

  return conteudo.mensagem ?? "Recebemos seus dados.";
}

export type Sessao = {
  id: string;
  nome: string;
  email: string;
  perfil: "ADMINISTRADOR" | "PASTOR" | "EQUIPE";
  igrejaId: string | null;
  deveTrocarSenha: boolean;
};

export type IgrejaInterna = {
  id: string;
  nome: string;
  logradouro: string;
  numero: string;
  bairro: string;
  complemento: string | null;
  cep: string;
  cidade: string;
  estado: string;
  ativa: boolean;
  identificadorPublico: string;
  identificadorPublicoMembros: string;
  criadaEm: string;
  atualizadaEm: string;
};

export type DadosIgreja = Pick<
  IgrejaInterna,
  "nome" | "logradouro" | "numero" | "bairro" | "complemento" | "cep" | "cidade" | "estado"
>;

export type UsuarioInterno = {
  id: string;
  nome: string;
  perfil: string;
};

export type PerfilUsuario = "ADMINISTRADOR" | "PASTOR" | "EQUIPE";

export type UsuarioGestao = {
  id: string;
  nome: string;
  email: string;
  perfil: PerfilUsuario;
  igrejaId: string | null;
  nomeIgreja: string | null;
  ativo: boolean;
  deveTrocarSenha: boolean;
  criadoEm: string;
  atualizadoEm: string;
};

export type NovoUsuario = {
  nome: string;
  email: string;
  perfil: PerfilUsuario;
  igrejaId: string | null;
  senhaTemporaria: string;
};

export type AtendimentoResumo = {
  id: string;
  nomeVisitante: string;
  whatsapp: string;
  querConhecerIgreja: boolean;
  querConversaOracao: boolean;
  etapa: EtapaAtendimento;
  responsavelId: string | null;
  nomeResponsavel: string | null;
  criadoEm: string;
  ultimaAtividadeEm: string;
  resultado: ResultadoAtendimento | null;
  concluidoEm: string | null;
};

export type EtapaAtendimento =
  | "NOVO"
  | "AGUARDANDO_CONTATO"
  | "EM_ATENDIMENTO"
  | "EM_ACOMPANHAMENTO"
  | "CONCLUIDO";

export type ResultadoAtendimento =
  | "TORNOU_SE_MEMBRO"
  | "ORACAO_ATENDIDA"
  | "NAO_DESEJA_PROSSEGUIR"
  | "SEM_RETORNO";

export type SituacaoCongregacional = "NAO_INFORMADO" | "NAO_CONGREGA" | "CONGREGA";

export type DadosComplementaresAtendimento = {
  logradouro: string | null;
  numeroResidencia: string | null;
  bairro: string | null;
  complemento: string | null;
  dataNascimento: string | null;
  situacaoCongregacional: SituacaoCongregacional;
  nomeIgrejaCongrega: string | null;
};

export type AtendimentoDetalhe = AtendimentoResumo & {
  igrejaId: string;
  nomeIgreja: string;
  logradouroVisitante: string | null;
  numeroResidencia: string | null;
  bairroVisitante: string | null;
  complementoEndereco: string | null;
  dataNascimento: string | null;
  situacaoCongregacional: SituacaoCongregacional;
  nomeIgrejaCongrega: string | null;
  observacoes: Array<{
    id: string;
    texto: string;
    criadaEm: string;
    nomeAutor: string;
  }>;
  historico: Array<{
    id: string;
    tipo: string;
    descricao: string;
    criadoEm: string;
    nomeAutor: string;
  }>;
};

let tokenCsrf: string | null = null;

async function obterTokenCsrf(): Promise<string> {
  if (tokenCsrf) {
    return tokenCsrf;
  }

  const resposta = await fetch("/api/seguranca/token-csrf");
  const conteudo = (await resposta.json()) as { token: string };
  tokenCsrf = conteudo.token;
  return tokenCsrf;
}

async function requisicao<T>(url: string, opcoes?: RequestInit): Promise<T> {
  const resposta = await fetch(url, opcoes);
  if (resposta.status === 204) {
    return undefined as T;
  }

  const conteudo = (await resposta.json().catch(() => ({}))) as {
    detail?: string;
    mensagem?: string;
    errors?: ErrosValidacao;
    duplicados?: Array<{ id: string; nome: string; sobrenome: string; situacao: string }>;
  };
  if (!resposta.ok) {
    throw new ErroDaApi(
      conteudo.detail ?? conteudo.mensagem ?? "Não foi possível concluir a operação.",
      conteudo.errors,
      resposta.status,
      conteudo.duplicados,
    );
  }

  return conteudo as T;
}

async function mutacao<T>(url: string, metodo: "POST" | "PUT", corpo?: unknown): Promise<T> {
  const token = await obterTokenCsrf();
  return requisicao<T>(url, {
    method: metodo,
    headers: {
      "Content-Type": "application/json",
      "X-CSRF-TOKEN": token,
    },
    body: corpo === undefined ? undefined : JSON.stringify(corpo),
  });
}

export function obterSessao(): Promise<Sessao> {
  return requisicao<Sessao>("/api/autenticacao/sessao");
}

export function entrar(email: string, senha: string): Promise<void> {
  return mutacao<void>("/api/autenticacao/entrar", "POST", { email, senha });
}

export function sair(): Promise<void> {
  return mutacao<void>("/api/autenticacao/sair", "POST");
}

export function alterarSenha(
  senhaAtual: string,
  novaSenha: string,
  confirmacaoSenha: string,
): Promise<void> {
  return mutacao<void>("/api/autenticacao/alterar-senha", "POST", {
    senhaAtual,
    novaSenha,
    confirmacaoSenha,
  });
}

export function listarIgrejas(): Promise<IgrejaInterna[]> {
  return requisicao<IgrejaInterna[]>("/api/igrejas");
}

export function criarIgreja(dados: DadosIgreja): Promise<{ id: string }> {
  return mutacao<{ id: string }>("/api/igrejas", "POST", dados);
}

export function editarIgreja(id: string, dados: DadosIgreja): Promise<void> {
  return mutacao<void>(`/api/igrejas/${id}`, "PUT", dados);
}

export function alterarSituacaoIgreja(id: string, ativo: boolean): Promise<void> {
  return mutacao<void>(`/api/igrejas/${id}/situacao`, "PUT", { ativo });
}

export function listarUsuarios(igrejaId: string): Promise<UsuarioInterno[]> {
  return requisicao<UsuarioInterno[]>(`/api/usuarios?igrejaId=${encodeURIComponent(igrejaId)}`);
}

export function listarUsuariosParaGestao(igrejaId: string | null): Promise<UsuarioGestao[]> {
  const consulta = igrejaId ? `?igrejaId=${encodeURIComponent(igrejaId)}` : "";
  return requisicao<UsuarioGestao[]>(`/api/usuarios/gestao${consulta}`);
}

export function obterSenhaSugerida(): Promise<{ senha: string }> {
  return requisicao<{ senha: string }>("/api/usuarios/senha-sugerida");
}

export function criarUsuario(dados: NovoUsuario): Promise<{ id: string }> {
  return mutacao<{ id: string }>("/api/usuarios", "POST", dados);
}

export function editarUsuario(
  id: string,
  dados: Pick<NovoUsuario, "nome" | "email" | "perfil">,
): Promise<void> {
  return mutacao<void>(`/api/usuarios/${id}`, "PUT", dados);
}

export function alterarSituacaoUsuario(id: string, ativo: boolean): Promise<void> {
  return mutacao<void>(`/api/usuarios/${id}/situacao`, "PUT", { ativo });
}

export function redefinirSenhaUsuario(id: string, senhaTemporaria: string): Promise<void> {
  return mutacao<void>(`/api/usuarios/${id}/redefinir-senha`, "POST", { senhaTemporaria });
}

export function listarAtendimentos(
  igrejaId: string,
  termo = "",
): Promise<AtendimentoResumo[]> {
  const pesquisa = termo.trim()
    ? `&termo=${encodeURIComponent(termo.trim())}`
    : "";
  return requisicao<AtendimentoResumo[]>(
    `/api/atendimentos?igrejaId=${encodeURIComponent(igrejaId)}${pesquisa}`,
  );
}

export function obterAtendimento(id: string, igrejaId: string): Promise<AtendimentoDetalhe> {
  return requisicao<AtendimentoDetalhe>(
    `/api/atendimentos/${id}?igrejaId=${encodeURIComponent(igrejaId)}`,
  );
}

export function adicionarObservacao(id: string, igrejaId: string, texto: string): Promise<void> {
  return mutacao<void>(
    `/api/atendimentos/${id}/observacoes?igrejaId=${encodeURIComponent(igrejaId)}`,
    "POST",
    { texto },
  );
}

export function alterarEtapa(
  id: string,
  igrejaId: string,
  etapa: EtapaAtendimento,
  resultado: ResultadoAtendimento | null,
): Promise<void> {
  return mutacao<void>(
    `/api/atendimentos/${id}/etapa?igrejaId=${encodeURIComponent(igrejaId)}`,
    "PUT",
    { etapa, resultado },
  );
}

export function atribuirResponsavel(
  id: string,
  igrejaId: string,
  responsavelId: string | null,
): Promise<void> {
  return mutacao<void>(
    `/api/atendimentos/${id}/responsavel?igrejaId=${encodeURIComponent(igrejaId)}`,
    "PUT",
    { responsavelId },
  );
}

export function atualizarDadosComplementares(
  id: string,
  igrejaId: string,
  dados: DadosComplementaresAtendimento,
): Promise<void> {
  return mutacao<void>(
    `/api/atendimentos/${id}/dados-complementares?igrejaId=${encodeURIComponent(igrejaId)}`,
    "PUT",
    dados,
  );
}

export type DadosMembro = {
  nome: string;
  sobrenome: string;
  whatsapp: string | null;
  email: string | null;
  dataNascimento: string | null;
  logradouro: string | null;
  numero: string | null;
  bairro: string | null;
  complemento: string | null;
  cep: string | null;
  cidade: string | null;
  estado: string | null;
  situacaoBatismo: "NAO_INFORMADO" | "BATIZADO" | "NAO_BATIZADO";
  dataBatismo: string | null;
  ehMenor: boolean;
  nomeResponsavelLegal: string | null;
  whatsappResponsavelLegal: string | null;
  vinculoResponsavelLegal: string | null;
};

export type MembroResumo = {
  id: string; nome: string; sobrenome: string; whatsapp: string | null;
  ehMenor: boolean; situacao: "ATIVO" | "INATIVO"; atualizadoEm: string;
};

export type SolicitacaoMembroResumo = {
  id: string; nome: string; sobrenome: string; whatsapp: string | null;
  estado: "PENDENTE" | "APROVADA" | "RECUSADA"; criadaEm: string;
};

export type HistoricoMembro = {
  acao: string; descricao: string | null; criadoEm: string; nomeAutor: string;
};

export type MembroDetalhe = {
  id: string; igrejaId: string; dados: DadosMembro; situacao: "ATIVO" | "INATIVO";
  origem: string; dataIngresso: string | null; observacaoPastoral: string | null;
  motivoInativacao: string | null; criadoEm: string; atualizadoEm: string;
  historico: HistoricoMembro[];
};

export type SolicitacaoMembroDetalhe = {
  id: string; igrejaId: string; dados: DadosMembro;
  estado: "PENDENTE" | "APROVADA" | "RECUSADA";
  avisoPrivacidadeVersao: string; criadaEm: string; atualizadaEm: string;
  decididaEm: string | null; motivoRecusa: string | null; membroId: string | null;
  historico: HistoricoMembro[];
};

export type Pagina<T> = { itens: T[]; total: number; pagina: number; tamanhoPagina: number };

export function obterFormularioMembro(identificador: string): Promise<IgrejaPublica> {
  return requisicao<IgrejaPublica>(`/api/publico/membros/${encodeURIComponent(identificador)}`);
}

export function enviarSolicitacaoMembro(identificador: string, dados: DadosMembro,
  versao: string, declaracaoMaioridade: boolean): Promise<{ mensagem: string }> {
  return requisicao<{ mensagem: string }>(
    `/api/publico/membros/${encodeURIComponent(identificador)}/solicitacoes`,
    { method: "POST", headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ dados, declaracaoMaioridade,
        avisoPrivacidadeReconhecido: true, avisoPrivacidadeVersao: versao }) },
  );
}

export function obterResumoMembros(igrejaId: string): Promise<{
  ativos: number; inativos: number; pendentes: number;
}> {
  return requisicao(`/api/membros/resumo?igrejaId=${encodeURIComponent(igrejaId)}`);
}

export function listarMembros(igrejaId: string, pagina: number, termo: string,
  situacao: string): Promise<Pagina<MembroResumo>> {
  const parametros = new URLSearchParams({ igrejaId, pagina: String(pagina) });
  if (termo) parametros.set("termo", termo);
  if (situacao) parametros.set("situacao", situacao);
  return requisicao(`/api/membros?${parametros}`);
}

export function listarSolicitacoesMembros(igrejaId: string, pagina: number):
  Promise<Pagina<SolicitacaoMembroResumo>> {
  return requisicao(`/api/membros/solicitacoes?igrejaId=${encodeURIComponent(igrejaId)}&pagina=${pagina}`);
}

export function obterMembro(igrejaId: string, id: string): Promise<MembroDetalhe> {
  return requisicao(`/api/membros/${id}?igrejaId=${encodeURIComponent(igrejaId)}`);
}

export function obterSolicitacaoMembro(igrejaId: string, id: string):
  Promise<SolicitacaoMembroDetalhe> {
  return requisicao(`/api/membros/solicitacoes/${id}?igrejaId=${encodeURIComponent(igrejaId)}`);
}

export type SalvarMembro = { dados: DadosMembro; dataIngresso: string | null;
  observacaoPastoral: string | null; confirmarNovoApesarDuplicidade: boolean };

export function criarMembro(igrejaId: string, dados: SalvarMembro): Promise<{ id: string }> {
  return mutacao(`/api/membros?igrejaId=${encodeURIComponent(igrejaId)}`, "POST", dados);
}

export function editarMembro(igrejaId: string, id: string, dados: SalvarMembro): Promise<void> {
  return mutacao(`/api/membros/${id}?igrejaId=${encodeURIComponent(igrejaId)}`, "PUT", dados);
}

export function alterarSituacaoMembro(igrejaId: string, id: string,
  ativo: boolean, motivo: string): Promise<void> {
  return mutacao(`/api/membros/${id}/situacao?igrejaId=${encodeURIComponent(igrejaId)}`,
    "PUT", { ativo, motivo });
}

export function corrigirSolicitacaoMembro(igrejaId: string, id: string,
  dados: DadosMembro): Promise<void> {
  return mutacao(`/api/membros/solicitacoes/${id}?igrejaId=${encodeURIComponent(igrejaId)}`,
    "PUT", { dados });
}

export function aprovarSolicitacaoMembro(igrejaId: string, id: string,
  membroExistenteId: string | null, confirmarNovoApesarDuplicidade: boolean,
  dataIngresso: string | null, observacaoPastoral: string | null): Promise<{ membroId: string }> {
  return mutacao(`/api/membros/solicitacoes/${id}/aprovar?igrejaId=${encodeURIComponent(igrejaId)}`,
    "POST", { membroExistenteId, confirmarNovoApesarDuplicidade,
      dataIngresso, observacaoPastoral });
}

export function recusarSolicitacaoMembro(igrejaId: string, id: string,
  motivo: string): Promise<void> {
  return mutacao(`/api/membros/solicitacoes/${id}/recusar?igrejaId=${encodeURIComponent(igrejaId)}`,
    "POST", { motivo });
}
