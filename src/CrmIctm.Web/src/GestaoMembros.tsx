import { useCallback, useEffect, useState, type FormEvent } from "react";
import {
  ErroDaApi, alterarSituacaoMembro, aprovarSolicitacaoMembro,
  corrigirSolicitacaoMembro, criarMembro, editarMembro,
  listarMembros, listarSolicitacoesMembros, obterMembro,
  obterResumoMembros, obterSolicitacaoMembro, recusarSolicitacaoMembro,
  type DadosMembro, type ErrosValidacao, type IgrejaInterna,
  type MembroDetalhe, type MembroResumo, type Pagina,
  type SolicitacaoMembroDetalhe, type SolicitacaoMembroResumo,
} from "./api";
import { CamposMembro, dadosMembroVazios } from "./CadastroMembro";
import { formatarTelefoneBrasileiro } from "./formatadores";

const paginaVazia = <T,>(): Pagina<T> => ({ itens: [], total: 0, pagina: 1, tamanhoPagina: 20 });

export function GestaoMembros({ igreja }: { igreja: IgrejaInterna }) {
  const [aba, setAba] = useState<"MEMBROS" | "SOLICITACOES">("MEMBROS");
  const [resumo, setResumo] = useState({ ativos: 0, inativos: 0, pendentes: 0 });
  const [membros, setMembros] = useState<Pagina<MembroResumo>>(paginaVazia());
  const [solicitacoes, setSolicitacoes] = useState<Pagina<SolicitacaoMembroResumo>>(paginaVazia());
  const [pagina, setPagina] = useState(1);
  const [textoPesquisa, setTextoPesquisa] = useState("");
  const [termo, setTermo] = useState("");
  const [situacao, setSituacao] = useState("");
  const [membro, setMembro] = useState<MembroDetalhe | null>(null);
  const [solicitacao, setSolicitacao] = useState<SolicitacaoMembroDetalhe | null>(null);
  const [novo, setNovo] = useState(false);
  const [dados, setDados] = useState<DadosMembro>({ ...dadosMembroVazios });
  const [dataIngresso, setDataIngresso] = useState("");
  const [observacao, setObservacao] = useState("");
  const [motivoRecusa, setMotivoRecusa] = useState("");
  const [motivoInativacao, setMotivoInativacao] = useState("");
  const [confirmandoInativacao, setConfirmandoInativacao] = useState(false);
  const [editando, setEditando] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState("");
  const [erros, setErros] = useState<ErrosValidacao>({});
  const [duplicados, setDuplicados] = useState<ErroDaApi["duplicados"]>([]);

  const carregar = useCallback(async () => {
    setCarregando(true);
    setErro("");
    try {
      const [resumoAtual, lista] = await Promise.all([
        obterResumoMembros(igreja.id),
        aba === "MEMBROS"
          ? listarMembros(igreja.id, pagina, termo, situacao)
          : listarSolicitacoesMembros(igreja.id, pagina),
      ]);
      setResumo(resumoAtual);
      if (aba === "MEMBROS") setMembros(lista as Pagina<MembroResumo>);
      else setSolicitacoes(lista as Pagina<SolicitacaoMembroResumo>);
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : "Não foi possível carregar os membros.");
    } finally {
      setCarregando(false);
    }
  }, [igreja.id, aba, pagina, termo, situacao]);

  useEffect(() => { void carregar(); }, [carregar]);
  useEffect(() => {
    setMembro(null); setSolicitacao(null); setNovo(false); setEditando(false);
    setPagina(1); setTermo(""); setTextoPesquisa(""); setSituacao("");
  }, [igreja.id]);

  function editarDados(dadosAtuais: DadosMembro, ingresso: string | null,
    observacaoAtual: string | null) {
    setDados({ ...dadosAtuais });
    setDataIngresso(ingresso ?? "");
    setObservacao(observacaoAtual ?? "");
    setErros({}); setDuplicados([]); setErro(""); setEditando(true);
  }

  async function abrirMembro(id: string) {
    setErro("");
    try {
      const detalhe = await obterMembro(igreja.id, id);
      setMembro(detalhe); setSolicitacao(null); setNovo(false); setEditando(false);
      setConfirmandoInativacao(false); setMotivoInativacao("");
    } catch (falha) { setErro(falha instanceof Error ? falha.message : "Não foi possível abrir o membro."); }
  }

  async function abrirSolicitacao(id: string) {
    setErro("");
    try {
      const detalhe = await obterSolicitacaoMembro(igreja.id, id);
      setSolicitacao(detalhe); setMembro(null); setNovo(false); setEditando(false);
      setDados({ ...detalhe.dados }); setDataIngresso(""); setObservacao("");
      setMotivoRecusa(""); setDuplicados([]);
    } catch (falha) { setErro(falha instanceof Error ? falha.message : "Não foi possível abrir a solicitação."); }
  }

  function tratarFalha(falha: unknown) {
    if (falha instanceof ErroDaApi) {
      setErro(falha.message);
      setErros(falha.erros);
      setDuplicados(falha.duplicados ?? []);
    } else setErro(falha instanceof Error ? falha.message : "Não foi possível salvar.");
  }

  async function salvarMembro(confirmarDuplicidade = false) {
    setSalvando(true); setErro(""); setErros({}); setDuplicados([]);
    try {
      const corpo = { dados, dataIngresso: dataIngresso || null,
        observacaoPastoral: observacao || null,
        confirmarNovoApesarDuplicidade: confirmarDuplicidade };
      if (membro) await editarMembro(igreja.id, membro.id, corpo);
      else await criarMembro(igreja.id, corpo);
      setNovo(false); setEditando(false);
      if (membro) setMembro(await obterMembro(igreja.id, membro.id));
      await carregar();
    } catch (falha) { tratarFalha(falha); }
    finally { setSalvando(false); }
  }

  async function salvarCorrecao() {
    if (!solicitacao) return;
    setSalvando(true); setErro(""); setErros({});
    try {
      await corrigirSolicitacaoMembro(igreja.id, solicitacao.id, dados);
      setSolicitacao(await obterSolicitacaoMembro(igreja.id, solicitacao.id));
      setEditando(false); await carregar();
    } catch (falha) { tratarFalha(falha); }
    finally { setSalvando(false); }
  }

  async function aprovar(membroExistenteId: string | null = null, confirmarNovo = false) {
    if (!solicitacao) return;
    setSalvando(true); setErro(""); setDuplicados([]);
    try {
      const resultado = await aprovarSolicitacaoMembro(igreja.id, solicitacao.id,
        membroExistenteId, confirmarNovo, dataIngresso || null, observacao || null);
      setSolicitacao(null); setAba("MEMBROS"); setPagina(1);
      await abrirMembro(resultado.membroId);
      await carregar();
    } catch (falha) { tratarFalha(falha); }
    finally { setSalvando(false); }
  }

  async function recusar() {
    if (!solicitacao) return;
    if (!motivoRecusa.trim()) { setErro("Informe o motivo da recusa."); return; }
    setSalvando(true); setErro("");
    try {
      await recusarSolicitacaoMembro(igreja.id, solicitacao.id, motivoRecusa);
      setSolicitacao(null); await carregar();
    } catch (falha) { tratarFalha(falha); }
    finally { setSalvando(false); }
  }

  async function trocarSituacao(ativo: boolean) {
    if (!membro) return;
    setSalvando(true); setErro("");
    try {
      await alterarSituacaoMembro(igreja.id, membro.id, ativo,
        ativo ? "" : motivoInativacao);
      setMembro(await obterMembro(igreja.id, membro.id)); await carregar();
      setConfirmandoInativacao(false); setMotivoInativacao("");
    } catch (falha) { tratarFalha(falha); }
    finally { setSalvando(false); }
  }

  const lista = aba === "MEMBROS" ? membros : solicitacoes;
  const totalPaginas = Math.max(1, Math.ceil(lista.total / lista.tamanhoPagina));

  return (
    <section className="gestao-membros" aria-labelledby="titulo-membros">
      <header className="cabecalho-gestao-membros">
        <div><p className="rotulo-painel">Comunidade · {igreja.nome}</p>
          <h1 id="titulo-membros">Gestão de membros</h1>
          <p>Cadastros organizados, com análise pastoral antes da aprovação.</p></div>
        <div className="acoes-gestao-membros">
          <a className="botao-secundario" href={`/qrcode-membros/${igreja.id}`}
            target="_blank" rel="noreferrer">Ver QR de membros</a>
          <button type="button" onClick={() => {
            setNovo(true); setMembro(null); setSolicitacao(null); setEditando(false);
            setDados({ ...dadosMembroVazios }); setDataIngresso(""); setObservacao("");
            setErro(""); setErros({}); setDuplicados([]);
          }}>Novo membro</button>
        </div>
      </header>

      <div className="indicadores-membros" aria-label="Resumo dos membros">
        <div><strong>{resumo.ativos}</strong><span>Ativos</span></div>
        <div><strong>{resumo.pendentes}</strong><span>Aguardando análise</span></div>
        <div><strong>{resumo.inativos}</strong><span>Inativos</span></div>
      </div>

      <div className="abas-membros" role="tablist" aria-label="Cadastros de membros">
        <button type="button" role="tab" aria-selected={aba === "MEMBROS"}
          className={aba === "MEMBROS" ? "ativo" : ""}
          onClick={() => { setAba("MEMBROS"); setPagina(1); setMembro(null); setSolicitacao(null); setNovo(false); }}>
          Membros</button>
        <button type="button" role="tab" aria-selected={aba === "SOLICITACOES"}
          className={aba === "SOLICITACOES" ? "ativo" : ""}
          onClick={() => { setAba("SOLICITACOES"); setPagina(1); setMembro(null); setSolicitacao(null); setNovo(false); }}>
          Solicitações {resumo.pendentes > 0 && <span>{resumo.pendentes}</span>}</button>
      </div>

      {aba === "MEMBROS" && (
        <form className="filtros-membros" onSubmit={(evento) => {
          evento.preventDefault(); setPagina(1); setTermo(textoPesquisa.trim());
        }} role="search">
          <input type="search" aria-label="Pesquisar por nome ou WhatsApp"
            placeholder="Nome ou WhatsApp" value={textoPesquisa}
            onChange={(evento) => setTextoPesquisa(evento.target.value)} />
          <select aria-label="Filtrar por situação" value={situacao}
            onChange={(evento) => { setSituacao(evento.target.value); setPagina(1); }}>
            <option value="">Todos</option><option value="ATIVO">Ativos</option>
            <option value="INATIVO">Inativos</option>
          </select>
          <button type="submit">Pesquisar</button>
        </form>
      )}

      {erro && <div className="alerta" role="alert">{erro}</div>}

      {novo || editando ? (
        <form className="cartao-gestao-membro" onSubmit={(evento) => {
          evento.preventDefault();
          if (solicitacao) void salvarCorrecao();
          else void salvarMembro();
        }}>
          <div className="topo-cartao-membro"><div>
            <p className="rotulo-painel">{solicitacao ? "Conferência pastoral" : membro ? "Atualização" : "Inclusão interna"}</p>
            <h2>{solicitacao ? "Corrigir solicitação" : membro ? "Editar membro" : "Novo membro"}</h2>
          </div><button type="button" className="botao-secundario"
            onClick={() => { setNovo(false); setEditando(false); setErro(""); }}>Cancelar</button></div>
          <CamposMembro dados={dados} alterar={setDados} erros={erros} interno={!solicitacao} />
          {!solicitacao && <div className="grade-campos-membro">
            <label className="campo-membro"><span>Data de ingresso (opcional)</span>
              <input type="date" value={dataIngresso}
                onChange={(evento) => setDataIngresso(evento.target.value)} /></label>
            <label className="campo-membro"><span>Observação pastoral (interna)</span>
              <textarea value={observacao} maxLength={2000}
                onChange={(evento) => setObservacao(evento.target.value)} /></label>
          </div>}
          <div className="acoes-cartao-membro"><button type="submit" disabled={salvando}>
            {salvando ? "Salvando…" : solicitacao ? "Salvar correção" : "Salvar membro"}
          </button></div>
          {duplicados.length > 0 && <div className="aviso-duplicidade">
            <strong>Confira antes de criar outro cadastro</strong>
            {duplicados.map((item) => <p key={item.id}>{item.nome} {item.sobrenome} · {item.situacao}</p>)}
            <button type="button" disabled={salvando}
              onClick={() => void salvarMembro(true)}>
              Confirmar cadastro separado</button>
          </div>}
        </form>
      ) : membro ? (
        <article className="cartao-gestao-membro">
          <div className="topo-cartao-membro"><div><p className="rotulo-painel">Membro {membro.situacao.toLowerCase()}</p>
            <h2>{membro.dados.nome} {membro.dados.sobrenome}</h2></div>
            <button type="button" className="botao-secundario" onClick={() => setMembro(null)}>Fechar</button></div>
          <ResumoDados dados={membro.dados} />
          <p><strong>Origem:</strong> {membro.origem === "FORMULARIO" ? "Formulário" : "Inclusão interna"}</p>
          {membro.dataIngresso && <p><strong>Ingresso:</strong> {membro.dataIngresso}</p>}
          {membro.observacaoPastoral && <p><strong>Observação pastoral:</strong> {membro.observacaoPastoral}</p>}
          {membro.motivoInativacao && <p><strong>Motivo da inativação:</strong> {membro.motivoInativacao}</p>}
          <Historico itens={membro.historico} />
          <div className="acoes-cartao-membro">
            <button type="button" onClick={() => editarDados(membro.dados,
              membro.dataIngresso, membro.observacaoPastoral)}>Editar dados</button>
            <button type="button" className="botao-secundario" disabled={salvando}
              onClick={() => membro.situacao === "ATIVO"
                ? setConfirmandoInativacao(true) : void trocarSituacao(true)}>
              {membro.situacao === "ATIVO" ? "Inativar" : "Reativar"}</button>
          </div>
          {confirmandoInativacao && <div className="recusa-membro">
            <label htmlFor="motivo-inativacao">Motivo da inativação (opcional)</label>
            <textarea id="motivo-inativacao" maxLength={500} value={motivoInativacao}
              onChange={(evento) => setMotivoInativacao(evento.target.value)} />
            <div className="acoes-cartao-membro">
              <button type="button" className="botao-secundario"
                onClick={() => setConfirmandoInativacao(false)}>Cancelar</button>
              <button type="button" disabled={salvando}
                onClick={() => void trocarSituacao(false)}>Confirmar inativação</button>
            </div>
          </div>}
        </article>
      ) : solicitacao ? (
        <article className="cartao-gestao-membro">
          <div className="topo-cartao-membro"><div><p className="rotulo-painel">Solicitação pendente</p>
            <h2>{solicitacao.dados.nome} {solicitacao.dados.sobrenome}</h2></div>
            <button type="button" className="botao-secundario" onClick={() => setSolicitacao(null)}>Fechar</button></div>
          <ResumoDados dados={solicitacao.dados} />
          <p><small>Enviada em {new Date(solicitacao.criadaEm).toLocaleDateString("pt-BR")} · Aviso {solicitacao.avisoPrivacidadeVersao}</small></p>
          <Historico itens={solicitacao.historico} />
          <div className="grade-campos-membro">
            <label className="campo-membro"><span>Data de ingresso (opcional)</span>
              <input type="date" value={dataIngresso}
                onChange={(evento) => setDataIngresso(evento.target.value)} /></label>
            <label className="campo-membro"><span>Observação pastoral (interna)</span>
              <textarea value={observacao} maxLength={2000}
                onChange={(evento) => setObservacao(evento.target.value)} /></label>
          </div>
          <div className="acoes-cartao-membro">
            <button type="button" className="botao-secundario"
              onClick={() => editarDados(solicitacao.dados, null, null)}>Corrigir dados</button>
            <button type="button" disabled={salvando} onClick={() => void aprovar()}>Aprovar</button>
          </div>
          {duplicados.length > 0 && <div className="aviso-duplicidade">
            <strong>Possível cadastro existente. Escolha como prosseguir:</strong>
            <small>Ao vincular, os dados do membro existente não serão alterados. Confira-os antes de decidir.</small>
            {duplicados.map((item) => <div key={item.id}>
              <span>{item.nome} {item.sobrenome} · {item.situacao}</span>
              <button type="button" onClick={() => void aprovar(item.id)}>
                Vincular a este membro</button></div>)}
            <button type="button" className="botao-secundario"
              onClick={() => void aprovar(null, true)}>Criar outro membro mesmo assim</button>
          </div>}
          <div className="recusa-membro">
            <label htmlFor="motivo-recusa">Motivo da recusa (interno)</label>
            <textarea id="motivo-recusa" value={motivoRecusa} maxLength={500}
              onChange={(evento) => setMotivoRecusa(evento.target.value)} />
            <button type="button" className="botao-secundario" disabled={salvando}
              onClick={() => void recusar()}>Recusar solicitação</button>
          </div>
        </article>
      ) : (
        <div className="lista-membros">
          {carregando && <p>Carregando cadastros…</p>}
          {!carregando && lista.itens.length === 0 && <p className="vazio-membros">
            {aba === "MEMBROS" ? "Nenhum membro encontrado." : "Nenhuma solicitação pendente."}</p>}
          {aba === "MEMBROS" && membros.itens.map((item) => (
            <button className="linha-membro" type="button" key={item.id}
              onClick={() => void abrirMembro(item.id)}>
              <span className="avatar-membro">{item.nome.charAt(0)}{item.sobrenome.charAt(0)}</span>
              <span className="nome-linha-membro"><strong>{item.nome} {item.sobrenome}</strong>
                <small>{item.whatsapp ? formatarTelefoneBrasileiro(item.whatsapp) : "Sem WhatsApp"}
                  {item.ehMenor ? " · Menor de idade" : ""}</small></span>
              <span className={`selo-membro ${item.situacao.toLowerCase()}`}>{item.situacao}</span>
              <span aria-hidden="true">›</span>
            </button>
          ))}
          {aba === "SOLICITACOES" && solicitacoes.itens.map((item) => (
            <button className="linha-membro" type="button" key={item.id}
              onClick={() => void abrirSolicitacao(item.id)}>
              <span className="avatar-membro">{item.nome.charAt(0)}{item.sobrenome.charAt(0)}</span>
              <span className="nome-linha-membro"><strong>{item.nome} {item.sobrenome}</strong>
                <small>{new Date(item.criadaEm).toLocaleDateString("pt-BR")} · {item.whatsapp ? formatarTelefoneBrasileiro(item.whatsapp) : ""}</small></span>
              <span className="selo-membro pendente">Pendente</span><span aria-hidden="true">›</span>
            </button>
          ))}
          {lista.total > 0 && <nav className="paginacao-membros" aria-label="Páginas de membros">
            <button type="button" disabled={pagina <= 1}
              onClick={() => setPagina((atual) => atual - 1)}>Anterior</button>
            <span>Página {pagina} de {totalPaginas} · {lista.total} registros</span>
            <button type="button" disabled={pagina >= totalPaginas}
              onClick={() => setPagina((atual) => atual + 1)}>Próxima</button>
          </nav>}
        </div>
      )}
    </section>
  );
}

function ResumoDados({ dados }: { dados: DadosMembro }) {
  const endereco = [dados.logradouro, dados.numero, dados.bairro,
    dados.complemento, dados.cep, dados.cidade, dados.estado].filter(Boolean).join(", ");
  return <dl className="resumo-dados-membro">
    <div><dt>WhatsApp</dt><dd>{dados.whatsapp ? formatarTelefoneBrasileiro(dados.whatsapp) : "Não informado"}</dd></div>
    <div><dt>E-mail</dt><dd>{dados.email || "Não informado"}</dd></div>
    <div><dt>Nascimento</dt><dd>{dados.dataNascimento || "Não informado"}</dd></div>
    <div><dt>Endereço</dt><dd>{endereco || "Não informado"}</dd></div>
    <div><dt>Batismo</dt><dd>{dados.situacaoBatismo === "BATIZADO" ? "Batizado" :
      dados.situacaoBatismo === "NAO_BATIZADO" ? "Não batizado" : "Não informado"}
      {dados.dataBatismo ? ` · ${dados.dataBatismo}` : ""}</dd></div>
    {dados.ehMenor && <div><dt>Responsável legal</dt><dd>{dados.nomeResponsavelLegal} · {dados.vinculoResponsavelLegal}
      {dados.whatsappResponsavelLegal ? ` · ${formatarTelefoneBrasileiro(dados.whatsappResponsavelLegal)}` : ""}</dd></div>}
  </dl>;
}

function Historico({ itens }: { itens: Array<{ acao: string; descricao: string | null;
  criadoEm: string; nomeAutor: string }> }) {
  return <div className="historico-membro"><h3>Histórico</h3>
    {itens.map((item, indice) => <p key={`${item.criadoEm}-${indice}`}>
      <strong>{item.acao.replaceAll("_", " ")}</strong> · {item.nomeAutor} · {new Date(item.criadoEm).toLocaleString("pt-BR")}
      {item.descricao && <small>{item.descricao}</small>}
    </p>)}
  </div>;
}
