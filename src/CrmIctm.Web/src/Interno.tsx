import { useCallback, useEffect, useRef, useState, type FormEvent } from "react";
import {
  DragDropProvider,
  useDraggable,
  useDroppable,
  type DragEndEvent,
} from "@dnd-kit/react";
import {
  ErroDaApi,
  adicionarObservacao,
  alterarEtapa,
  alterarSenha,
  atribuirResponsavel,
  atualizarDadosComplementares,
  entrar,
  listarAtendimentos,
  listarIgrejas,
  listarUsuarios,
  obterAtendimento,
  obterSessao,
  sair,
  type AtendimentoDetalhe,
  type AtendimentoResumo,
  type EtapaAtendimento,
  type IgrejaInterna,
  type ResultadoAtendimento,
  type Sessao,
  type SituacaoCongregacional,
  type UsuarioInterno,
} from "./api";
import { GestaoIgrejas, GestaoUsuarios } from "./Administracao";
import { GestaoMembros } from "./GestaoMembros";
import {
  obterIgrejaSelecionadaDaSessao,
  obterIgrejaSelecionadaDoEndereco,
  obterUltimaIgrejaSelecionadaDoUsuario,
  salvarIgrejaSelecionada,
} from "./contextoIgreja";
import { formatarTelefoneBrasileiro } from "./formatadores";
import { LogoIgreja } from "./LogoIgreja";

const etapas: Array<{ codigo: EtapaAtendimento; nome: string }> = [
  { codigo: "NOVO", nome: "Novo visitante" },
  { codigo: "AGUARDANDO_CONTATO", nome: "Aguardando contato" },
  { codigo: "EM_ATENDIMENTO", nome: "Em atendimento" },
  { codigo: "EM_ACOMPANHAMENTO", nome: "Em acompanhamento" },
  { codigo: "CONCLUIDO", nome: "Concluído" },
];

const resultados: Array<{ codigo: ResultadoAtendimento; nome: string }> = [
  { codigo: "TORNOU_SE_MEMBRO", nome: "Tornou-se membro" },
  { codigo: "ORACAO_ATENDIDA", nome: "Oração atendida" },
  { codigo: "NAO_DESEJA_PROSSEGUIR", nome: "Não deseja prosseguir" },
  { codigo: "SEM_RETORNO", nome: "Sem retorno" },
];

type DadosArrasteAtendimento = {
  tipo: "ATENDIMENTO";
  atendimentoId: string;
  etapa: EtapaAtendimento;
};

type DadosDestinoEtapa = {
  tipo: "ETAPA";
  etapa: EtapaAtendimento;
};

function etapasSaoAdjacentes(origem: EtapaAtendimento, destino: EtapaAtendimento) {
  const indiceOrigem = etapas.findIndex((item) => item.codigo === origem);
  const indiceDestino = etapas.findIndex((item) => item.codigo === destino);
  return indiceOrigem >= 0 && indiceDestino >= 0 && Math.abs(indiceOrigem - indiceDestino) === 1;
}

function obterEtapasPermitidas(etapaAtual: EtapaAtendimento) {
  const indiceAtual = etapas.findIndex((item) => item.codigo === etapaAtual);
  return etapas.filter((_, indice) => Math.abs(indice - indiceAtual) <= 1);
}

export function PaginaLogin() {
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState("");
  const [enviando, setEnviando] = useState(false);

  async function autenticar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    setErro("");
    setEnviando(true);
    try {
      await entrar(email, senha);
      window.location.replace(`/painel?atualizacao=${Date.now()}`);
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : "Não foi possível entrar.");
    } finally {
      setEnviando(false);
    }
  }

  return (
    <main className="pagina-mensagem">
      <div className="estrutura-login">
        <section className="mensagem login">
          <div className="cabecalho-login">
            <LogoIgreja />
            <div><small>Área da equipe</small><strong>Gestão da igreja</strong></div>
          </div>
          <h1>Que bom ter você de volta</h1>
          <p>Entre com a conta fornecida pela sua igreja.</p>
          {erro && <div className="alerta" role="alert">{erro}</div>}
          <form onSubmit={autenticar}>
            <label htmlFor="email">E-mail</label>
            <input
              id="email"
              type="email"
              autoComplete="username"
              required
              value={email}
              onChange={(evento) => setEmail(evento.target.value)}
            />
            <label htmlFor="senha">Senha</label>
            <input
              id="senha"
              type="password"
              autoComplete="current-password"
              required
              value={senha}
              onChange={(evento) => setSenha(evento.target.value)}
            />
            <button type="submit" disabled={enviando}>
              {enviando ? "Entrando…" : "Entrar"}
            </button>
          </form>
        </section>
        <footer className="rodape-login">Acesso restrito à equipe autorizada</footer>
      </div>
    </main>
  );
}

function PaginaTrocaSenha({ usuario }: { usuario: Sessao }) {
  const [senhaAtual, setSenhaAtual] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [confirmacao, setConfirmacao] = useState("");
  const [erro, setErro] = useState("");
  const [salvando, setSalvando] = useState(false);

  async function salvar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    setSalvando(true);
    setErro("");
    try {
      await alterarSenha(senhaAtual, novaSenha, confirmacao);
      window.location.reload();
    } catch (falha) {
      if (falha instanceof ErroDaApi) {
        setErro(Object.values(falha.erros).flat()[0] ?? falha.message);
      } else {
        setErro(falha instanceof Error ? falha.message : "Não foi possível alterar a senha.");
      }
    } finally {
      setSalvando(false);
    }
  }

  return (
    <main className="pagina-mensagem">
      <div className="estrutura-login">
        <section className="mensagem login troca-senha">
          <div className="cabecalho-login">
            <LogoIgreja />
            <div><small>Primeiro acesso</small><strong>Proteja sua conta</strong></div>
          </div>
          <h1>Olá, {usuario.nome}</h1>
          <p>Defina uma senha pessoal antes de acessar o painel.</p>
          {erro && <div className="alerta" role="alert">{erro}</div>}
          <form onSubmit={salvar}>
            <label htmlFor="senha-atual">Senha temporária</label>
            <input id="senha-atual" type="password" autoComplete="current-password" required value={senhaAtual} onChange={(evento) => setSenhaAtual(evento.target.value)} />
            <label htmlFor="nova-senha">Nova senha</label>
            <input id="nova-senha" type="password" autoComplete="new-password" required value={novaSenha} onChange={(evento) => setNovaSenha(evento.target.value)} />
            <small className="ajuda-senha">Use ao menos 10 caracteres, com maiúscula, minúscula, número e símbolo.</small>
            <label htmlFor="confirmar-senha">Confirmar nova senha</label>
            <input id="confirmar-senha" type="password" autoComplete="new-password" required value={confirmacao} onChange={(evento) => setConfirmacao(evento.target.value)} />
            <button type="submit" disabled={salvando}>{salvando ? "Salvando…" : "Salvar e acessar o painel"}</button>
          </form>
        </section>
      </div>
    </main>
  );
}

export function PainelAtendimentos() {
  const [sessao, setSessao] = useState<Sessao | null>(null);
  const [igrejas, setIgrejas] = useState<IgrejaInterna[]>([]);
  const [igrejaId, setIgrejaId] = useState("");
  const [atendimentos, setAtendimentos] = useState<AtendimentoResumo[]>([]);
  const [detalhe, setDetalhe] = useState<AtendimentoDetalhe | null>(null);
  const [usuarios, setUsuarios] = useState<UsuarioInterno[]>([]);
  const [erro, setErro] = useState("");
  const [carregando, setCarregando] = useState(true);
  const [pesquisa, setPesquisa] = useState("");
  const [termoPesquisa, setTermoPesquisa] = useState("");
  const [movendoAtendimentoId, setMovendoAtendimentoId] = useState<string | null>(null);
  const [conclusaoPendente, setConclusaoPendente] = useState<AtendimentoResumo | null>(null);
  const [resultadoConclusao, setResultadoConclusao] = useState<ResultadoAtendimento | "">("");
  const [secao, setSecao] = useState<"ATENDIMENTOS" | "MEMBROS" | "IGREJAS" | "USUARIOS">("ATENDIMENTOS");
  const igrejaIdAtual = useRef("");

  const carregarAtendimentos = useCallback(async (idIgreja: string, termo = "") => {
    if (!idIgreja) return;
    const [lista, equipe] = await Promise.all([
      listarAtendimentos(idIgreja, termo),
      listarUsuarios(idIgreja),
    ]);
    if (igrejaIdAtual.current !== idIgreja) return;
    setAtendimentos(lista);
    setUsuarios(equipe);
  }, []);

  useEffect(() => {
    const iniciar = async () => {
      try {
        const sessaoAtual = await obterSessao();
        setSessao(sessaoAtual);
        if (sessaoAtual.deveTrocarSenha) {
          return;
        }
        const igrejasDisponiveis = await listarIgrejas();
        const idSolicitado = obterIgrejaSelecionadaDoEndereco()
          ?? obterUltimaIgrejaSelecionadaDoUsuario(sessaoAtual.id)
          ?? obterIgrejaSelecionadaDaSessao();
        const idInicial = sessaoAtual.igrejaId
          ?? igrejasDisponiveis.find((igreja) => igreja.id === idSolicitado)?.id
          ?? igrejasDisponiveis.find((igreja) => igreja.ativa)?.id
          ?? igrejasDisponiveis[0]?.id
          ?? "";
        setIgrejas(igrejasDisponiveis);
        igrejaIdAtual.current = idInicial;
        setIgrejaId(idInicial);
        if (idInicial) {
          salvarIgrejaSelecionada(
            idInicial,
            sessaoAtual.perfil === "ADMINISTRADOR",
            sessaoAtual.id,
          );
        }
        await carregarAtendimentos(idInicial);
      } catch (falha) {
        if (falha instanceof ErroDaApi && falha.status === 401) {
          const igrejaDoEndereco = obterIgrejaSelecionadaDoEndereco();
          if (igrejaDoEndereco) salvarIgrejaSelecionada(igrejaDoEndereco);
          window.location.replace("/entrar");
          return;
        }
        setErro(falha instanceof Error ? falha.message : "Não foi possível carregar o painel.");
      } finally {
        setCarregando(false);
      }
    };
    void iniciar();
  }, [carregarAtendimentos]);

  async function selecionarIgreja(novoId: string) {
    if (!igrejas.some((igreja) => igreja.id === novoId)) {
      setErro("A igreja selecionada não está disponível para este usuário.");
      return;
    }

    igrejaIdAtual.current = novoId;
    setIgrejaId(novoId);
    salvarIgrejaSelecionada(novoId, sessao?.perfil === "ADMINISTRADOR", sessao?.id);
    setDetalhe(null);
    setAtendimentos([]);
    setUsuarios([]);
    setPesquisa("");
    setTermoPesquisa("");
    setErro("");
    setCarregando(true);
    try {
      await carregarAtendimentos(novoId);
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : "Não foi possível carregar os atendimentos.");
    } finally {
      setCarregando(false);
    }
  }

  async function atualizarIgrejas() {
    const igrejasAtualizadas = await listarIgrejas();
    setIgrejas(igrejasAtualizadas);
    if (!igrejasAtualizadas.some((igreja) => igreja.id === igrejaId)) {
      const novoId = igrejasAtualizadas.find((igreja) => igreja.ativa)?.id
        ?? igrejasAtualizadas[0]?.id
        ?? "";
      igrejaIdAtual.current = novoId;
      setIgrejaId(novoId);
      if (novoId) {
        salvarIgrejaSelecionada(novoId, sessao?.perfil === "ADMINISTRADOR", sessao?.id);
      }
    }
  }

  async function abrirDetalhe(id: string) {
    try {
      setDetalhe(await obterAtendimento(id, igrejaId));
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : "Não foi possível abrir o atendimento.");
    }
  }

  async function atualizarDetalhe() {
    if (!detalhe) return;
    const atualizado = await obterAtendimento(detalhe.id, igrejaId);
    setDetalhe(atualizado);
    await carregarAtendimentos(igrejaId, termoPesquisa);
  }

  async function salvarMovimento(
    atendimentoId: string,
    destino: EtapaAtendimento,
    resultado: ResultadoAtendimento | null = null,
  ) {
    const atendimento = atendimentos.find((item) => item.id === atendimentoId);
    if (!atendimento || !etapasSaoAdjacentes(atendimento.etapa, destino)) return false;

    const etapaAnterior = atendimento.etapa;
    setMovendoAtendimentoId(atendimentoId);
    setErro("");
    setAtendimentos((atuais) => atuais.map((item) => (
      item.id === atendimentoId ? { ...item, etapa: destino } : item
    )));

    try {
      await alterarEtapa(atendimentoId, igrejaId, destino, resultado);
      await carregarAtendimentos(igrejaId, termoPesquisa);
      if (detalhe?.id === atendimentoId) {
        setDetalhe(await obterAtendimento(atendimentoId, igrejaId));
      }
      return true;
    } catch (falha) {
      setAtendimentos((atuais) => atuais.map((item) => (
        item.id === atendimentoId ? { ...item, etapa: etapaAnterior } : item
      )));
      setErro(falha instanceof Error ? falha.message : "Não foi possível mover o atendimento.");
      return false;
    } finally {
      setMovendoAtendimentoId(null);
    }
  }

  function finalizarArraste(evento: DragEndEvent) {
    if (evento.canceled) return;
    const origem = evento.operation.source?.data as DadosArrasteAtendimento | undefined;
    const destino = evento.operation.target?.data as DadosDestinoEtapa | undefined;
    if (origem?.tipo !== "ATENDIMENTO" || destino?.tipo !== "ETAPA") return;
    if (!etapasSaoAdjacentes(origem.etapa, destino.etapa)) return;

    if (destino.etapa === "CONCLUIDO") {
      const atendimento = atendimentos.find((item) => item.id === origem.atendimentoId);
      if (!atendimento) return;
      setResultadoConclusao("");
      setConclusaoPendente(atendimento);
      return;
    }

    void salvarMovimento(origem.atendimentoId, destino.etapa);
  }

  async function confirmarConclusaoPorArraste() {
    if (!conclusaoPendente || !resultadoConclusao) return;
    const concluido = await salvarMovimento(
      conclusaoPendente.id,
      "CONCLUIDO",
      resultadoConclusao,
    );
    if (concluido) {
      setConclusaoPendente(null);
      setResultadoConclusao("");
    }
  }

  async function pesquisarAtendimentos(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const termo = pesquisa.trim();
    setTermoPesquisa(termo);
    setDetalhe(null);
    setErro("");
    try {
      await carregarAtendimentos(igrejaId, termo);
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : "Não foi possível realizar a pesquisa.");
    }
  }

  async function limparPesquisa() {
    setPesquisa("");
    setTermoPesquisa("");
    setDetalhe(null);
    setErro("");
    try {
      await carregarAtendimentos(igrejaId);
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : "Não foi possível limpar a pesquisa.");
    }
  }

  if (carregando && !sessao) {
    return <main className="pagina-painel"><p>Carregando painel…</p></main>;
  }

  if (sessao?.deveTrocarSenha) {
    return <PaginaTrocaSenha usuario={sessao} />;
  }

  return (
    <main className="pagina-painel">
      <header className="topo-painel">
        <div className="identidade-painel">
          <LogoIgreja />
          <div><strong>Gestão da igreja</strong><small>Olá, {sessao?.nome}</small></div>
        </div>
        <div className="acoes-topo">
          {(secao === "ATENDIMENTOS" || secao === "MEMBROS") && sessao?.perfil === "ADMINISTRADOR" && (
            <SeletorIgreja
              igrejas={igrejas}
              igrejaId={igrejaId}
              desabilitado={carregando}
              aoSelecionar={selecionarIgreja}
            />
          )}
          {secao === "ATENDIMENTOS" && igrejaId && (
            <a
              className="botao-secundario link-topo"
              href={`/qrcode/${igrejaId}`}
              target="_blank"
              rel="noreferrer"
            >
              Ver convite e QR Code
            </a>
          )}
          <button
            className="botao-secundario"
            onClick={() => void sair().then(() => window.location.replace(`/entrar?atualizacao=${Date.now()}`))}
          >
            Sair
          </button>
        </div>
      </header>

      <nav className="navegacao-interna" aria-label="Seções do sistema">
        <button type="button" className={secao === "ATENDIMENTOS" ? "ativo" : ""} onClick={() => setSecao("ATENDIMENTOS")}>Atendimentos</button>
        {(sessao?.perfil === "ADMINISTRADOR" || sessao?.perfil === "PASTOR") && (
          <button type="button" className={secao === "MEMBROS" ? "ativo" : ""}
            onClick={() => { setSecao("MEMBROS"); setDetalhe(null); }}>Membros</button>
        )}
        {sessao?.perfil === "ADMINISTRADOR" && (
          <button type="button" className={secao === "IGREJAS" ? "ativo" : ""} onClick={() => { setSecao("IGREJAS"); setDetalhe(null); }}>Igrejas</button>
        )}
        {(sessao?.perfil === "ADMINISTRADOR" || sessao?.perfil === "PASTOR") && (
          <button type="button" className={secao === "USUARIOS" ? "ativo" : ""} onClick={() => { setSecao("USUARIOS"); setDetalhe(null); }}>Usuários</button>
        )}
      </nav>

      {erro && <div className="alerta alerta-painel" role="alert">{erro}</div>}

      {secao === "ATENDIMENTOS" && <>
      <section className="apresentacao-painel">
        <div>
          <p className="rotulo-painel">Painel de acompanhamento</p>
          <h1>Cuidado em cada etapa</h1>
          <p>Acompanhe os visitantes e mantenha a equipe alinhada.</p>
        </div>
        <div className="resumo-painel" aria-label="Resumo dos atendimentos">
          <div><strong>{atendimentos.length}</strong><span>Total</span></div>
          <div><strong>{atendimentos.filter((item) => item.etapa === "NOVO").length}</strong><span>Novos</span></div>
          <div><strong>{atendimentos.filter((item) => item.etapa === "EM_ATENDIMENTO").length}</strong><span>Em atendimento</span></div>
        </div>
      </section>

      <form className="barra-pesquisa" role="search" onSubmit={pesquisarAtendimentos}>
        <label htmlFor="pesquisa-atendimentos">Pesquisar visitante</label>
        <div>
          <input
            id="pesquisa-atendimentos"
            type="search"
            placeholder="Nome ou WhatsApp"
            value={pesquisa}
            onChange={(evento) => setPesquisa(evento.target.value)}
          />
          <button type="submit">Pesquisar</button>
          {termoPesquisa && (
            <button type="button" className="botao-limpar-pesquisa" onClick={() => void limparPesquisa()}>
              Limpar
            </button>
          )}
        </div>
        {termoPesquisa && <small>Resultados para “{termoPesquisa}”</small>}
      </form>

      <DragDropProvider onDragEnd={finalizarArraste}>
        <section className="kanban" aria-label="Atendimentos por etapa">
          {etapas.map((etapa) => (
            <ColunaKanban
              key={etapa.codigo}
              etapa={etapa}
              atendimentos={atendimentos.filter((item) => item.etapa === etapa.codigo)}
              movendoAtendimentoId={movendoAtendimentoId}
              aoAbrir={abrirDetalhe}
            />
          ))}
        </section>
      </DragDropProvider>

      {conclusaoPendente && (
        <div
          className="sobreposicao sobreposicao-dialogo"
          role="presentation"
          onMouseDown={(evento) => evento.target === evento.currentTarget && setConclusaoPendente(null)}
        >
          <section className="dialogo-conclusao" role="dialog" aria-modal="true" aria-labelledby="titulo-conclusao-arraste">
            <p className="rotulo-painel">Concluir atendimento</p>
            <h2 id="titulo-conclusao-arraste">Qual foi o resultado?</h2>
            <p>Informe o resultado do atendimento de {conclusaoPendente.nomeVisitante} antes de concluir.</p>
            <label htmlFor="resultado-conclusao-arraste">Resultado</label>
            <select
              id="resultado-conclusao-arraste"
              autoFocus
              value={resultadoConclusao}
              onChange={(evento) => setResultadoConclusao(evento.target.value as ResultadoAtendimento)}
            >
              <option value="">Selecione o resultado</option>
              {resultados.map((item) => <option key={item.codigo} value={item.codigo}>{item.nome}</option>)}
            </select>
            <div className="acoes-dialogo">
              <button type="button" className="botao-secundario" onClick={() => setConclusaoPendente(null)}>Cancelar</button>
              <button
                type="button"
                disabled={!resultadoConclusao || movendoAtendimentoId === conclusaoPendente.id}
                onClick={() => void confirmarConclusaoPorArraste()}
              >
                {movendoAtendimentoId === conclusaoPendente.id ? "Concluindo…" : "Concluir atendimento"}
              </button>
            </div>
          </section>
        </div>
      )}

      {detalhe && (
        <DetalheAtendimento
          atendimento={detalhe}
          igrejaId={igrejaId}
          usuarios={usuarios}
          aoFechar={() => setDetalhe(null)}
          aoAtualizar={atualizarDetalhe}
        />
      )}
      </>}

      {secao === "IGREJAS" && sessao?.perfil === "ADMINISTRADOR" && (
        <GestaoIgrejas igrejas={igrejas} aoAtualizar={atualizarIgrejas} />
      )}

      {secao === "MEMBROS" && sessao && (sessao.perfil === "ADMINISTRADOR" || sessao.perfil === "PASTOR") &&
        igrejas.find((item) => item.id === igrejaId) && (
        <GestaoMembros key={igrejaId} igreja={igrejas.find((item) => item.id === igrejaId)!} />
      )}

      {secao === "USUARIOS" && sessao && (sessao.perfil === "ADMINISTRADOR" || sessao.perfil === "PASTOR") && (
        <GestaoUsuarios sessao={sessao} igrejas={igrejas} />
      )}

      <footer className="rodape-painel">
        <span>IEC · Gestão da igreja</span>
        <span>Ambiente interno e protegido</span>
      </footer>
    </main>
  );
}

function SeletorIgreja({
  igrejas,
  igrejaId,
  desabilitado,
  aoSelecionar,
}: {
  igrejas: IgrejaInterna[];
  igrejaId: string;
  desabilitado: boolean;
  aoSelecionar: (igrejaId: string) => Promise<void>;
}) {
  const [aberto, setAberto] = useState(false);
  const seletorRef = useRef<HTMLDivElement>(null);
  const igrejaSelecionada = igrejas.find((igreja) => igreja.id === igrejaId);

  useEffect(() => {
    if (!aberto) return;

    const fecharAoClicarFora = (evento: PointerEvent) => {
      if (!seletorRef.current?.contains(evento.target as Node)) {
        setAberto(false);
      }
    };
    const fecharComEscape = (evento: KeyboardEvent) => {
      if (evento.key === "Escape") {
        setAberto(false);
      }
    };

    document.addEventListener("pointerdown", fecharAoClicarFora);
    document.addEventListener("keydown", fecharComEscape);
    return () => {
      document.removeEventListener("pointerdown", fecharAoClicarFora);
      document.removeEventListener("keydown", fecharComEscape);
    };
  }, [aberto]);

  async function selecionar(novoId: string) {
    setAberto(false);
    if (novoId !== igrejaId) {
      await aoSelecionar(novoId);
    }
  }

  return (
    <div className={`seletor-igreja${aberto ? " aberto" : ""}`} ref={seletorRef}>
      <button
        type="button"
        className="gatilho-seletor-igreja"
        aria-haspopup="true"
        aria-expanded={aberto}
        aria-controls="opcoes-igrejas"
        disabled={desabilitado || igrejas.length === 0}
        onClick={() => setAberto((atual) => !atual)}
      >
        <span className="icone-seletor-igreja" aria-hidden="true">⌂</span>
        <span className="texto-seletor-igreja">
          <strong>{igrejaSelecionada?.nome ?? "Selecione uma igreja"}</strong>
        </span>
        <span className="seta-seletor-igreja" aria-hidden="true">⌄</span>
      </button>

      {aberto && (
        <div id="opcoes-igrejas" className="opcoes-seletor-igreja" role="group" aria-label="Escolha uma igreja">
          <div className="cabecalho-opcoes-igreja">
            <strong>Trocar igreja</strong>
            <small>{igrejas.length} {igrejas.length === 1 ? "igreja disponível" : "igrejas disponíveis"}</small>
          </div>
          <div className="lista-opcoes-igreja">
            {igrejas.map((igreja) => {
              const selecionada = igreja.id === igrejaId;
              return (
                <button
                  key={igreja.id}
                  type="button"
                  className={`opcao-seletor-igreja${selecionada ? " selecionada" : ""}`}
                  aria-pressed={selecionada}
                  onClick={() => void selecionar(igreja.id)}
                >
                  <span className="marcador-opcao-igreja" aria-hidden="true">{selecionada ? "✓" : ""}</span>
                  <span>
                    <strong>{igreja.nome}</strong>
                    <small>{igreja.cidade} · {igreja.estado}</small>
                  </span>
                  {!igreja.ativa && <em>Inativa</em>}
                </button>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}

function ColunaKanban({
  etapa,
  atendimentos,
  movendoAtendimentoId,
  aoAbrir,
}: {
  etapa: { codigo: EtapaAtendimento; nome: string };
  atendimentos: AtendimentoResumo[];
  movendoAtendimentoId: string | null;
  aoAbrir: (id: string) => Promise<void>;
}) {
  const { ref, isDropTarget } = useDroppable<DadosDestinoEtapa>({
    id: `etapa-${etapa.codigo}`,
    data: { tipo: "ETAPA", etapa: etapa.codigo },
    accept: (origem) => {
      const dados = origem.data as DadosArrasteAtendimento | undefined;
      return dados?.tipo === "ATENDIMENTO" && etapasSaoAdjacentes(dados.etapa, etapa.codigo);
    },
  });

  return (
    <section
      ref={ref}
      className={`coluna-kanban${isDropTarget ? " destino-arraste" : ""}`}
      data-etapa={etapa.codigo}
    >
      <header>
        <div className="titulo-coluna"><span className="indicador-etapa" /><h2>{etapa.nome}</h2></div>
        <span>{atendimentos.length}</span>
      </header>
      <div className="lista-cartoes">
        {atendimentos.map((atendimento) => (
          <CartaoAtendimento
            key={atendimento.id}
            atendimento={atendimento}
            desabilitado={movendoAtendimentoId === atendimento.id}
            aoAbrir={aoAbrir}
          />
        ))}
        {atendimentos.length === 0 && <p className="coluna-vazia">Nenhum atendimento</p>}
      </div>
    </section>
  );
}

function CartaoAtendimento({
  atendimento,
  desabilitado,
  aoAbrir,
}: {
  atendimento: AtendimentoResumo;
  desabilitado: boolean;
  aoAbrir: (id: string) => Promise<void>;
}) {
  const { ref, handleRef, isDragging } = useDraggable<DadosArrasteAtendimento>({
    id: `atendimento-${atendimento.id}`,
    data: {
      tipo: "ATENDIMENTO",
      atendimentoId: atendimento.id,
      etapa: atendimento.etapa,
    },
    disabled: desabilitado,
  });

  return (
    <article ref={ref} className={`cartao-atendimento${isDragging ? " em-arraste" : ""}`}>
      <button
        type="button"
        className="conteudo-cartao"
        disabled={desabilitado}
        onClick={() => void aoAbrir(atendimento.id)}
      >
        <strong>{atendimento.nomeVisitante}</strong>
        <span>{formatarTelefoneBrasileiro(atendimento.whatsapp)}</span>
        <span className="etiqueta-interesse">{resumirInteresses(atendimento)}</span>
        <small className="responsavel-cartao">
          <span aria-hidden="true">●</span>{atendimento.nomeResponsavel ?? "Sem responsável"}
        </small>
      </button>
      <button
        ref={handleRef}
        type="button"
        className="alca-arraste"
        disabled={desabilitado}
        aria-label={`Mover ${atendimento.nomeVisitante}. Arraste para uma etapa adjacente.`}
        title="Arrastar para a etapa anterior ou seguinte"
      >
        <span aria-hidden="true">⠿</span>
      </button>
    </article>
  );
}

function DetalheAtendimento({
  atendimento,
  igrejaId,
  usuarios,
  aoFechar,
  aoAtualizar,
}: {
  atendimento: AtendimentoDetalhe;
  igrejaId: string;
  usuarios: UsuarioInterno[];
  aoFechar: () => void;
  aoAtualizar: () => Promise<void>;
}) {
  const [texto, setTexto] = useState("");
  const [etapa, setEtapa] = useState<EtapaAtendimento>(atendimento.etapa);
  const [resultado, setResultado] = useState<ResultadoAtendimento | "">(atendimento.resultado ?? "");
  const [logradouro, setLogradouro] = useState(atendimento.logradouroVisitante ?? "");
  const [numeroResidencia, setNumeroResidencia] = useState(atendimento.numeroResidencia ?? "");
  const [bairro, setBairro] = useState(atendimento.bairroVisitante ?? "");
  const [complemento, setComplemento] = useState(atendimento.complementoEndereco ?? "");
  const [dataNascimento, setDataNascimento] = useState(atendimento.dataNascimento ?? "");
  const [situacaoCongregacional, setSituacaoCongregacional] = useState<SituacaoCongregacional>(
    atendimento.situacaoCongregacional,
  );
  const [nomeIgrejaCongrega, setNomeIgrejaCongrega] = useState(atendimento.nomeIgrejaCongrega ?? "");
  const [erro, setErro] = useState("");
  const [salvando, setSalvando] = useState(false);
  const linkWhatsapp = `https://wa.me/${atendimento.whatsapp.replace(/\D/g, "")}`;

  useEffect(() => {
    setEtapa(atendimento.etapa);
    setResultado(atendimento.resultado ?? "");
    setLogradouro(atendimento.logradouroVisitante ?? "");
    setNumeroResidencia(atendimento.numeroResidencia ?? "");
    setBairro(atendimento.bairroVisitante ?? "");
    setComplemento(atendimento.complementoEndereco ?? "");
    setDataNascimento(atendimento.dataNascimento ?? "");
    setSituacaoCongregacional(atendimento.situacaoCongregacional);
    setNomeIgrejaCongrega(atendimento.nomeIgrejaCongrega ?? "");
  }, [atendimento]);

  async function executar(operacao: () => Promise<void>) {
    setSalvando(true);
    setErro("");
    try {
      await operacao();
      await aoAtualizar();
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : "Não foi possível salvar a alteração.");
    } finally {
      setSalvando(false);
    }
  }

  return (
    <div className="sobreposicao" role="presentation" onMouseDown={(evento) => evento.target === evento.currentTarget && aoFechar()}>
      <article className="detalhe-atendimento" role="dialog" aria-modal="true" aria-labelledby="titulo-detalhe">
        <header>
          <div><p>Atendimento</p><h2 id="titulo-detalhe">{atendimento.nomeVisitante}</h2></div>
          <button className="fechar" aria-label="Fechar" onClick={aoFechar}>×</button>
        </header>
        {erro && <div className="alerta" role="alert">{erro}</div>}

        <dl className="dados-atendimento">
          <div><dt>Igreja</dt><dd>{atendimento.nomeIgreja}</dd></div>
          <div><dt>WhatsApp</dt><dd>{formatarTelefoneBrasileiro(atendimento.whatsapp)}</dd></div>
          <div><dt>Interesses</dt><dd>{resumirInteresses(atendimento)}</dd></div>
          <div><dt>Entrada</dt><dd>{formatarData(atendimento.criadoEm)}</dd></div>
          <div><dt>Última atividade</dt><dd>{formatarData(atendimento.ultimaAtividadeEm)}</dd></div>
          {atendimento.resultado && (
            <div><dt>Resultado</dt><dd>{nomeResultado(atendimento.resultado)}</dd></div>
          )}
          {atendimento.concluidoEm && (
            <div><dt>Concluído em</dt><dd>{formatarData(atendimento.concluidoEm)}</dd></div>
          )}
        </dl>

        <a className="link-whatsapp" href={linkWhatsapp} target="_blank" rel="noreferrer">
          Abrir conversa no WhatsApp
        </a>

        <section className="bloco-detalhe dados-complementares">
          <div className="cabecalho-bloco-detalhe">
            <div>
              <h3>Informações do visitante</h3>
              <p>Dados opcionais informados durante o atendimento.</p>
            </div>
          </div>
          <div className="grade-dados-complementares">
            <label className="campo-detalhe campo-detalhe-logradouro">
              Rua/Avenida
              <input value={logradouro} maxLength={150} onChange={(evento) => setLogradouro(evento.target.value)} />
            </label>
            <label className="campo-detalhe">
              Número
              <input value={numeroResidencia} maxLength={20} onChange={(evento) => setNumeroResidencia(evento.target.value)} />
            </label>
            <label className="campo-detalhe">
              Bairro
              <input value={bairro} maxLength={100} onChange={(evento) => setBairro(evento.target.value)} />
            </label>
            <label className="campo-detalhe">
              Complemento
              <input value={complemento} maxLength={100} onChange={(evento) => setComplemento(evento.target.value)} />
            </label>
            <label className="campo-detalhe">
              Data de nascimento
              <input
                type="date"
                value={dataNascimento}
                max={new Date().toISOString().slice(0, 10)}
                onChange={(evento) => setDataNascimento(evento.target.value)}
              />
            </label>
            <label className="campo-detalhe">
              Situação congregacional
              <select
                value={situacaoCongregacional}
                onChange={(evento) => setSituacaoCongregacional(evento.target.value as SituacaoCongregacional)}
              >
                <option value="NAO_INFORMADO">Não informado</option>
                <option value="NAO_CONGREGA">Não congrega</option>
                <option value="CONGREGA">Congrega</option>
              </select>
            </label>
            {situacaoCongregacional === "CONGREGA" && (
              <label className="campo-detalhe campo-detalhe-largo">
                Igreja onde congrega
                <input value={nomeIgrejaCongrega} maxLength={150} onChange={(evento) => setNomeIgrejaCongrega(evento.target.value)} />
              </label>
            )}
          </div>
          <button
            type="button"
            disabled={salvando}
            onClick={() => void executar(() => atualizarDadosComplementares(
              atendimento.id,
              igrejaId,
              {
                logradouro: logradouro || null,
                numeroResidencia: numeroResidencia || null,
                bairro: bairro || null,
                complemento: complemento || null,
                dataNascimento: dataNascimento || null,
                situacaoCongregacional,
                nomeIgrejaCongrega: situacaoCongregacional === "CONGREGA"
                  ? nomeIgrejaCongrega || null
                  : null,
              },
            ))}
          >
            Salvar informações
          </button>
        </section>

        <section className="bloco-detalhe">
          <h3>Responsável</h3>
          <select
            value={atendimento.responsavelId ?? ""}
            disabled={salvando}
            onChange={(evento) => void executar(() => atribuirResponsavel(atendimento.id, igrejaId, evento.target.value || null))}
          >
            <option value="">Sem responsável</option>
            {usuarios.map((usuario) => <option key={usuario.id} value={usuario.id}>{usuario.nome}</option>)}
          </select>
        </section>

        <section className="bloco-detalhe">
          <h3>Etapa</h3>
          <select value={etapa} onChange={(evento) => setEtapa(evento.target.value as EtapaAtendimento)}>
            {obterEtapasPermitidas(atendimento.etapa).map((item) => (
              <option key={item.codigo} value={item.codigo}>{item.nome}</option>
            ))}
          </select>
          {etapa === "CONCLUIDO" && (
            <select value={resultado} onChange={(evento) => setResultado(evento.target.value as ResultadoAtendimento)}>
              <option value="">Selecione o resultado</option>
              {resultados.map((item) => <option key={item.codigo} value={item.codigo}>{item.nome}</option>)}
            </select>
          )}
          <button
            type="button"
            disabled={salvando || etapa === atendimento.etapa || (etapa === "CONCLUIDO" && !resultado)}
            onClick={() => void executar(() => alterarEtapa(atendimento.id, igrejaId, etapa, resultado || null))}
          >
            Salvar etapa
          </button>
        </section>

        <section className="bloco-detalhe">
          <h3>Nova observação</h3>
          <textarea value={texto} maxLength={2000} onChange={(evento) => setTexto(evento.target.value)} />
          <button
            type="button"
            disabled={salvando || !texto.trim()}
            onClick={() => void executar(async () => {
              await adicionarObservacao(atendimento.id, igrejaId, texto);
              setTexto("");
            })}
          >
            Adicionar observação
          </button>
        </section>

        <section className="bloco-detalhe">
          <h3>Observações</h3>
          {atendimento.observacoes.map((item) => (
            <article className="registro" key={item.id}>
              <p>{item.texto}</p><small>{item.nomeAutor} · {formatarData(item.criadaEm)}</small>
            </article>
          ))}
          {atendimento.observacoes.length === 0 && <p className="texto-neutro">Nenhuma observação.</p>}
        </section>

        <section className="bloco-detalhe">
          <h3>Histórico</h3>
          {atendimento.historico.map((item) => (
            <article className="registro" key={item.id}>
              <p>{item.descricao}</p><small>{item.nomeAutor} · {formatarData(item.criadoEm)}</small>
            </article>
          ))}
          {atendimento.historico.length === 0 && <p className="texto-neutro">Nenhuma alteração registrada.</p>}
        </section>
      </article>
    </div>
  );
}

function formatarData(valor: string) {
  return new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "short" }).format(new Date(valor));
}

function nomeResultado(resultado: ResultadoAtendimento) {
  return resultados.find((item) => item.codigo === resultado)?.nome ?? resultado;
}

function resumirInteresses(atendimento: Pick<AtendimentoResumo, "querConhecerIgreja" | "querConversaOracao">) {
  const interesses = [];
  if (atendimento.querConhecerIgreja) interesses.push("Conhecer a igreja");
  if (atendimento.querConversaOracao) interesses.push("Conversa e oração");
  return interesses.join(" · ");
}
