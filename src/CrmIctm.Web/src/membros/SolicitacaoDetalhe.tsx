import { useState } from "react";
import { rotuloSituacaoMembro } from "../formatadores";
import { CamposPastorais, Historico, ResumoDados, TopoCartao } from "./MembroPartes";
import type { ErroDaApi, SolicitacaoMembroDetalhe as Solicitacao } from "../api";

export function SolicitacaoDetalhe({
  solicitacao, salvando, duplicados, dataIngresso, observacao,
  aoVoltar, aoCorrigir, aoAlterarIngresso, aoAlterarObservacao, aoAprovar, aoRecusar,
}: {
  solicitacao: Solicitacao;
  salvando: boolean;
  duplicados: ErroDaApi["duplicados"];
  dataIngresso: string;
  observacao: string;
  aoVoltar: () => void;
  aoCorrigir: () => void;
  aoAlterarIngresso: (valor: string) => void;
  aoAlterarObservacao: (valor: string) => void;
  aoAprovar: (membroExistenteId?: string | null, confirmarNovo?: boolean) => void;
  aoRecusar: (motivo: string) => void;
}) {
  const [motivoRecusa, setMotivoRecusa] = useState("");

  return (
    <article className="cartao-gestao-membro">
      <TopoCartao rotulo="Solicitação pendente"
        titulo={`${solicitacao.dados.nome} ${solicitacao.dados.sobrenome}`}>
        <button type="button" className="botao-secundario voltar-lista-membros"
          onClick={aoVoltar}>← Voltar à lista</button>
      </TopoCartao>

      <ResumoDados dados={solicitacao.dados} />

      <p><small>
        Enviada em {new Date(solicitacao.criadaEm).toLocaleDateString("pt-BR")}
        {" · "}Aviso {solicitacao.avisoPrivacidadeVersao}
      </small></p>

      <Historico itens={solicitacao.historico} />

      <CamposPastorais dataIngresso={dataIngresso} observacao={observacao}
        aoAlterarIngresso={aoAlterarIngresso} aoAlterarObservacao={aoAlterarObservacao} />

      <div className="acoes-cartao-membro">
        <button type="button" className="botao-secundario" onClick={aoCorrigir}>Corrigir dados</button>
        <button type="button" disabled={salvando} onClick={() => aoAprovar()}>Aprovar</button>
      </div>

      {duplicados.length > 0 && (
        <div className="aviso-duplicidade">
          <strong>Possível cadastro existente. Escolha como prosseguir:</strong>
          <small>Ao vincular, os dados do membro existente não serão alterados. Confira-os antes de decidir.</small>
          {duplicados.map((item) => (
            <div key={item.id}>
              <span>{item.nome} {item.sobrenome} · {rotuloSituacaoMembro(item.situacao)}</span>
              <button type="button" onClick={() => aoAprovar(item.id)}>Vincular a este membro</button>
            </div>
          ))}
          <button type="button" className="botao-secundario"
            onClick={() => aoAprovar(null, true)}>Criar outro membro mesmo assim</button>
        </div>
      )}

      <div className="recusa-membro">
        <label htmlFor="motivo-recusa">Motivo da recusa (interno)</label>
        <textarea id="motivo-recusa" value={motivoRecusa} maxLength={500}
          onChange={(evento) => setMotivoRecusa(evento.target.value)} />
        <button type="button" className="botao-secundario" disabled={salvando}
          onClick={() => aoRecusar(motivoRecusa)}>Recusar solicitação</button>
      </div>
    </article>
  );
}
