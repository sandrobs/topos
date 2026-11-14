import { useCallback, useEffect, useState } from "react";
import {
  ErroDaApi, alterarSituacaoMembro, aprovarSolicitacaoMembro,
  corrigirSolicitacaoMembro, criarMembro, editarMembro,
  listarMembros, listarSolicitacoesMembros, obterMembro,
  obterResumoMembros, obterSolicitacaoMembro, recusarSolicitacaoMembro,
  type DadosMembro, type ErrosValidacao, type IgrejaInterna,
  type MembroDetalhe as Membro, type MembroResumo, type Pagina,
  type SolicitacaoMembroDetalhe as Solicitacao, type SolicitacaoMembroResumo,
} from "./api";
import { dadosMembroVazios } from "./CadastroMembro";
import { MembroDetalhe } from "./membros/MembroDetalhe";
import { MembroFormulario, type OrigemFormulario } from "./membros/MembroFormulario";
import { MembrosLista, type Aba, type Filtros } from "./membros/MembrosLista";
import { SolicitacaoDetalhe } from "./membros/SolicitacaoDetalhe";

const paginaVazia = <T,>(): Pagina<T> => ({ itens: [], total: 0, pagina: 1, tamanhoPagina: 20 });

/**
 * Qual das cinco telas está aberta.
 *
 * Antes isto eram quatro estados soltos — `novo`, `editando`, `membro` e
 * `solicitacao` — combinados numa cadeia ternária. Nada impedia dois deles
 * verdadeiros ao mesmo tempo, e cada transição precisava lembrar de zerar os
 * outros três. Com um estado só, a tela aberta é sempre uma, por construção.
 */
type Tela =
  | { nome: "LISTA" }
  | { nome: "MEMBRO"; membro: Membro }
  | { nome: "SOLICITACAO"; solicitacao: Solicitacao }
  | { nome: "FORMULARIO"; origem: OrigemFormulario; membro: Membro | null; solicitacao: Solicitacao | null };

export function GestaoMembros({ igreja }: { igreja: IgrejaInterna }) {
  // Listagem
  const [aba, setAba] = useState<Aba>("MEMBROS");
  const [resumo, setResumo] = useState({ ativos: 0, inativos: 0, pendentes: 0 });
  const [membros, setMembros] = useState<Pagina<MembroResumo>>(paginaVazia());
  const [solicitacoes, setSolicitacoes] = useState<Pagina<SolicitacaoMembroResumo>>(paginaVazia());
  const [pagina, setPagina] = useState(1);
  const [filtros, setFiltros] = useState<Filtros>({ texto: "", situacao: "" });
  const [termo, setTermo] = useState("");
  const [carregando, setCarregando] = useState(false);

  // Navegação
  const [tela, setTela] = useState<Tela>({ nome: "LISTA" });

  // Edição
  const [dados, setDados] = useState<DadosMembro>({ ...dadosMembroVazios });
  const [dataIngresso, setDataIngresso] = useState("");
  const [observacao, setObservacao] = useState("");
  const [erros, setErros] = useState<ErrosValidacao>({});
  const [duplicados, setDuplicados] = useState<ErroDaApi["duplicados"]>([]);

  // Transversais
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState("");

  const carregar = useCallback(async () => {
    setCarregando(true);
    try {
      const [resumoAtual, lista] = await Promise.all([
        obterResumoMembros(igreja.id),
        aba === "MEMBROS"
          ? listarMembros(igreja.id, pagina, termo, filtros.situacao)
          : listarSolicitacoesMembros(igreja.id, pagina),
      ]);
      setResumo(resumoAtual);
      if (aba === "MEMBROS") setMembros(lista as Pagina<MembroResumo>);
      else setSolicitacoes(lista as Pagina<SolicitacaoMembroResumo>);
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : "Não foi possível carregar os cadastros.");
    } finally {
      setCarregando(false);
    }
  }, [igreja.id, aba, pagina, termo, filtros.situacao]);

  useEffect(() => { void carregar(); }, [carregar]);

  function limparEdicao() {
    setDados({ ...dadosMembroVazios });
    setDataIngresso("");
    setObservacao("");
    setErro("");
    setErros({});
    setDuplicados([]);
  }

  function voltarParaLista() {
    setTela({ nome: "LISTA" });
    limparEdicao();
    void carregar();
  }

  function tratarFalha(falha: unknown) {
    setErro(falha instanceof Error ? falha.message : "Não foi possível concluir a operação.");
    if (falha instanceof ErroDaApi) {
      setErros(falha.erros);
      setDuplicados(falha.duplicados);
    }
  }

  async function abrirMembro(id: string) {
    setErro("");
    try {
      setTela({ nome: "MEMBRO", membro: await obterMembro(igreja.id, id) });
    } catch (falha) { tratarFalha(falha); }
  }

  async function abrirSolicitacao(id: string) {
    setErro("");
    setDuplicados([]);
    try {
      setTela({ nome: "SOLICITACAO", solicitacao: await obterSolicitacaoMembro(igreja.id, id) });
    } catch (falha) { tratarFalha(falha); }
  }

  function abrirFormulario(origem: OrigemFormulario) {
    limparEdicao();
    if (origem === "NOVO") {
      setTela({ nome: "FORMULARIO", origem, membro: null, solicitacao: null });
      return;
    }
    if (origem === "EDICAO" && tela.nome === "MEMBRO") {
      setDados(tela.membro.dados);
      setDataIngresso(tela.membro.dataIngresso ?? "");
      setObservacao(tela.membro.observacaoPastoral ?? "");
      setTela({ nome: "FORMULARIO", origem, membro: tela.membro, solicitacao: null });
      return;
    }
    if (origem === "CORRECAO" && tela.nome === "SOLICITACAO") {
      setDados(tela.solicitacao.dados);
      setTela({ nome: "FORMULARIO", origem, membro: null, solicitacao: tela.solicitacao });
    }
  }

  async function salvarMembro(confirmarDuplicidade = false) {
    if (tela.nome !== "FORMULARIO") return;
    setSalvando(true);
    setErro("");
    setErros({});
    setDuplicados([]);
    try {
      const corpo = {
        dados,
        dataIngresso: dataIngresso || null,
        observacaoPastoral: observacao || null,
        confirmarNovoApesarDuplicidade: confirmarDuplicidade,
      };
      if (tela.membro) await editarMembro(igreja.id, tela.membro.id, corpo);
      else await criarMembro(igreja.id, corpo);
      voltarParaLista();
    } catch (falha) { tratarFalha(falha); }
    finally { setSalvando(false); }
  }

  async function salvarCorrecao() {
    if (tela.nome !== "FORMULARIO" || !tela.solicitacao) return;
    setSalvando(true);
    setErro("");
    setErros({});
    try {
      await corrigirSolicitacaoMembro(igreja.id, tela.solicitacao.id, dados);
      await abrirSolicitacao(tela.solicitacao.id);
    } catch (falha) { tratarFalha(falha); }
    finally { setSalvando(false); }
  }

  async function aprovar(membroExistenteId: string | null = null, confirmarNovo = false) {
    if (tela.nome !== "SOLICITACAO") return;
    setSalvando(true);
    setErro("");
    setDuplicados([]);
    try {
      await aprovarSolicitacaoMembro(igreja.id, tela.solicitacao.id,
        membroExistenteId, confirmarNovo, dataIngresso || null, observacao || null);
      voltarParaLista();
    } catch (falha) { tratarFalha(falha); }
    finally { setSalvando(false); }
  }

  async function recusar(motivo: string) {
    if (tela.nome !== "SOLICITACAO") return;
    setSalvando(true);
    setErro("");
    try {
      await recusarSolicitacaoMembro(igreja.id, tela.solicitacao.id, motivo);
      voltarParaLista();
    } catch (falha) { tratarFalha(falha); }
    finally { setSalvando(false); }
  }

  async function trocarSituacao(ativo: boolean, motivo: string) {
    if (tela.nome !== "MEMBRO") return;
    setSalvando(true);
    setErro("");
    try {
      await alterarSituacaoMembro(igreja.id, tela.membro.id, ativo, motivo);
      await abrirMembro(tela.membro.id);
      void carregar();
    } catch (falha) { tratarFalha(falha); }
    finally { setSalvando(false); }
  }

  function trocarAba(nova: Aba) {
    setAba(nova);
    setPagina(1);
    setTela({ nome: "LISTA" });
  }

  return (
    <section className="gestao-membros" aria-labelledby="titulo-membros">
      <header className="apresentacao-painel">
        <div>
          <p className="rotulo-painel">Comunidade · {igreja.nome}</p>
          <h1 id="titulo-membros">Gestão de membros</h1>
          <p>Cadastros organizados, com análise pastoral antes da aprovação.</p>
        </div>
        <div className="lado-painel">
          <div className="acoes-painel">
            <a className="botao-secundario" href={`/qrcode-membros/${igreja.id}`}
              target="_blank" rel="noreferrer">Ver QR de membros</a>
            <button type="button" className="botao-principal"
              onClick={() => abrirFormulario("NOVO")}>Novo membro</button>
          </div>
          <div className="resumo-painel" aria-label="Resumo dos membros">
            <div><strong>{resumo.ativos}</strong><span>Ativos</span></div>
            <div><strong>{resumo.pendentes}</strong><span>Aguardando</span></div>
            <div><strong>{resumo.inativos}</strong><span>Inativos</span></div>
          </div>
        </div>
      </header>

      {erro && <div className="alerta" role="alert">{erro}</div>}

      {tela.nome === "FORMULARIO" && (
        <MembroFormulario
          origem={tela.origem}
          dados={dados}
          erros={erros}
          duplicados={duplicados}
          dataIngresso={dataIngresso}
          observacao={observacao}
          salvando={salvando}
          aoAlterarDados={setDados}
          aoAlterarIngresso={setDataIngresso}
          aoAlterarObservacao={setObservacao}
          aoEnviar={() => { if (tela.origem === "CORRECAO") void salvarCorrecao(); else void salvarMembro(); }}
          aoConfirmarDuplicidade={() => void salvarMembro(true)}
          aoCancelar={voltarParaLista}
        />
      )}

      {tela.nome === "MEMBRO" && (
        <MembroDetalhe
          membro={tela.membro}
          salvando={salvando}
          aoVoltar={voltarParaLista}
          aoEditar={() => abrirFormulario("EDICAO")}
          aoTrocarSituacao={(ativo, motivo) => void trocarSituacao(ativo, motivo)}
        />
      )}

      {tela.nome === "SOLICITACAO" && (
        <SolicitacaoDetalhe
          solicitacao={tela.solicitacao}
          salvando={salvando}
          duplicados={duplicados}
          dataIngresso={dataIngresso}
          observacao={observacao}
          aoVoltar={voltarParaLista}
          aoCorrigir={() => abrirFormulario("CORRECAO")}
          aoAlterarIngresso={setDataIngresso}
          aoAlterarObservacao={setObservacao}
          aoAprovar={(id, confirmar) => void aprovar(id ?? null, confirmar ?? false)}
          aoRecusar={(motivo) => void recusar(motivo)}
        />
      )}

      {tela.nome === "LISTA" && (
        <MembrosLista
          aba={aba}
          pendentes={resumo.pendentes}
          membros={membros}
          solicitacoes={solicitacoes}
          filtros={filtros}
          pagina={pagina}
          carregando={carregando}
          aoTrocarAba={trocarAba}
          aoAlterarFiltros={(novos) => {
            setFiltros(novos);
            if (novos.situacao !== filtros.situacao) setPagina(1);
          }}
          aoPesquisar={() => { setPagina(1); setTermo(filtros.texto.trim()); }}
          aoTrocarPagina={setPagina}
          aoAbrirMembro={(id) => void abrirMembro(id)}
          aoAbrirSolicitacao={(id) => void abrirSolicitacao(id)}
        />
      )}
    </section>
  );
}
