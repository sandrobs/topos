import { soltarFocoDePonteiro } from "../foco";
import { formatarTelefoneBrasileiro, rotuloSituacaoMembro } from "../formatadores";
import type { MembroResumo, Pagina, SolicitacaoMembroResumo } from "../api";

export type Aba = "MEMBROS" | "SOLICITACOES";

export type Filtros = { texto: string; situacao: string };

export function MembrosLista({
  aba, pendentes, membros, solicitacoes, filtros, pagina, carregando,
  aoTrocarAba, aoAlterarFiltros, aoPesquisar, aoTrocarPagina,
  aoAbrirMembro, aoAbrirSolicitacao,
}: {
  aba: Aba;
  pendentes: number;
  membros: Pagina<MembroResumo>;
  solicitacoes: Pagina<SolicitacaoMembroResumo>;
  filtros: Filtros;
  pagina: number;
  carregando: boolean;
  aoTrocarAba: (aba: Aba) => void;
  aoAlterarFiltros: (filtros: Filtros) => void;
  aoPesquisar: () => void;
  aoTrocarPagina: (pagina: number) => void;
  aoAbrirMembro: (id: string) => void;
  aoAbrirSolicitacao: (id: string) => void;
}) {
  const lista = aba === "MEMBROS" ? membros : solicitacoes;
  const totalPaginas = Math.max(1, Math.ceil(lista.total / lista.tamanhoPagina));

  return (
    <>
      <div className="abas-membros" role="tablist" aria-label="Cadastros de membros">
        <button type="button" role="tab" aria-selected={aba === "MEMBROS"}
          className={aba === "MEMBROS" ? "ativo" : ""}
          onClick={(evento) => { aoTrocarAba("MEMBROS"); soltarFocoDePonteiro(evento); }}>Membros</button>
        <button type="button" role="tab" aria-selected={aba === "SOLICITACOES"}
          className={aba === "SOLICITACOES" ? "ativo" : ""}
          onClick={(evento) => { aoTrocarAba("SOLICITACOES"); soltarFocoDePonteiro(evento); }}>
          Solicitações {pendentes > 0 && <span>{pendentes}</span>}
        </button>
      </div>

      {aba === "MEMBROS" && (
        <form className="filtros-membros" role="search"
          onSubmit={(evento) => { evento.preventDefault(); aoPesquisar(); }}>
          <input type="search" aria-label="Pesquisar por nome ou WhatsApp"
            placeholder="Nome ou WhatsApp" value={filtros.texto}
            onChange={(evento) => aoAlterarFiltros({ ...filtros, texto: evento.target.value })} />
          <select aria-label="Filtrar por situação" value={filtros.situacao}
            onChange={(evento) => aoAlterarFiltros({ ...filtros, situacao: evento.target.value })}>
            <option value="">Todos</option>
            <option value="ATIVO">Ativos</option>
            <option value="INATIVO">Inativos</option>
          </select>
          <button type="submit" disabled={carregando}>
            {carregando ? "Pesquisando…" : "Pesquisar"}
          </button>
        </form>
      )}

      <div className="lista-membros" aria-busy={carregando}>
        <p className="estado-lista" role="status" aria-live="polite">
          {carregando ? "Carregando cadastros…" : ""}
        </p>

        {!carregando && lista.itens.length === 0 && (
          <p className="vazio-membros">
            {aba === "MEMBROS" ? "Nenhum membro encontrado." : "Nenhuma solicitação pendente."}
          </p>
        )}

        {!carregando && aba === "MEMBROS" && membros.itens.map((item) => (
          <button className="linha-membro" type="button" key={item.id}
            onClick={() => aoAbrirMembro(item.id)}>
            <span className="avatar-membro">{item.nome.charAt(0)}{item.sobrenome.charAt(0)}</span>
            <span className="nome-linha-membro">
              <strong>{item.nome} {item.sobrenome}</strong>
              <small>
                {item.whatsapp ? formatarTelefoneBrasileiro(item.whatsapp) : "Sem WhatsApp"}
                {item.ehMenor ? " · Menor de idade" : ""}
              </small>
            </span>
            <span className={`selo-membro ${item.situacao.toLowerCase()}`}>
              {rotuloSituacaoMembro(item.situacao)}
            </span>
            <span aria-hidden="true">›</span>
          </button>
        ))}

        {!carregando && aba === "SOLICITACOES" && solicitacoes.itens.map((item) => (
          <button className="linha-membro" type="button" key={item.id}
            onClick={() => aoAbrirSolicitacao(item.id)}>
            <span className="avatar-membro">{item.nome.charAt(0)}{item.sobrenome.charAt(0)}</span>
            <span className="nome-linha-membro">
              <strong>{item.nome} {item.sobrenome}</strong>
              <small>
                {new Date(item.criadaEm).toLocaleDateString("pt-BR")}
                {item.whatsapp ? ` · ${formatarTelefoneBrasileiro(item.whatsapp)}` : ""}
              </small>
            </span>
            <span className="selo-membro pendente">Pendente</span>
            <span aria-hidden="true">›</span>
          </button>
        ))}

        {lista.total > 0 && (
          <nav className="paginacao-membros" aria-label="Páginas de membros">
            <button type="button" disabled={pagina <= 1}
              onClick={() => aoTrocarPagina(pagina - 1)}>Anterior</button>
            <span>Página {pagina} de {totalPaginas} · {lista.total} registros</span>
            <button type="button" disabled={pagina >= totalPaginas}
              onClick={() => aoTrocarPagina(pagina + 1)}>Próxima</button>
          </nav>
        )}
      </div>
    </>
  );
}
