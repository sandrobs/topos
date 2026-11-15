import { useState } from "react";
import { formatarDataIso } from "../formatadores";
import { Historico, ResumoDados, TopoCartao } from "./MembroPartes";
import type { MembroDetalhe as Membro } from "../api";

export function MembroDetalhe({
  membro, salvando, aoVoltar, aoEditar, aoTrocarSituacao,
}: {
  membro: Membro;
  salvando: boolean;
  aoVoltar: () => void;
  aoEditar: () => void;
  aoTrocarSituacao: (ativo: boolean, motivo: string) => void;
}) {
  const [confirmando, setConfirmando] = useState(false);
  const [motivo, setMotivo] = useState("");
  const ativo = membro.situacao === "ATIVO";

  return (
    <article className="cartao-gestao-membro">
      <TopoCartao rotulo={`Membro ${membro.situacao.toLowerCase()}`}
        titulo={`${membro.dados.nome} ${membro.dados.sobrenome}`}>
        <button type="button" className="botao-secundario voltar-lista-membros"
          onClick={aoVoltar}>← Voltar à lista</button>
      </TopoCartao>

      <ResumoDados dados={membro.dados} />

      <p><strong>Origem:</strong> {membro.origem === "FORMULARIO" ? "Formulário" : "Inclusão interna"}</p>
      {membro.dataIngresso && (
        <p><strong>Ingresso:</strong> {formatarDataIso(membro.dataIngresso)}</p>
      )}
      {membro.observacaoPastoral && (
        <p><strong>Observação pastoral:</strong> {membro.observacaoPastoral}</p>
      )}
      {membro.motivoInativacao && (
        <p><strong>Motivo da inativação:</strong> {membro.motivoInativacao}</p>
      )}

      <Historico itens={membro.historico} />

      <div className="acoes-cartao-membro">
        <button type="button" onClick={aoEditar}>Editar dados</button>
        <button type="button" className="botao-secundario" disabled={salvando}
          onClick={() => (ativo ? setConfirmando(true) : aoTrocarSituacao(true, ""))}>
          {ativo ? "Inativar" : "Reativar"}
        </button>
      </div>

      {confirmando && (
        <div className="recusa-membro">
          <label htmlFor="motivo-inativacao">Motivo da inativação (opcional)</label>
          <textarea id="motivo-inativacao" maxLength={500} value={motivo}
            onChange={(evento) => setMotivo(evento.target.value)} />
          <div className="acoes-cartao-membro">
            <button type="button" className="botao-secundario"
              onClick={() => { setConfirmando(false); setMotivo(""); }}>Cancelar</button>
            <button type="button" disabled={salvando}
              onClick={() => aoTrocarSituacao(false, motivo)}>Confirmar inativação</button>
          </div>
        </div>
      )}
    </article>
  );
}
