import { CamposMembro } from "../CadastroMembro";
import { rotuloSituacaoMembro } from "../formatadores";
import { CamposPastorais, TopoCartao } from "./MembroPartes";
import type { DadosMembro, ErroDaApi, ErrosValidacao } from "../api";

/** De onde o formulário foi aberto: muda o texto e o que o envio faz. */
export type OrigemFormulario = "NOVO" | "EDICAO" | "CORRECAO";

const textos: Record<OrigemFormulario, { rotulo: string; titulo: string; enviar: string }> = {
  NOVO: { rotulo: "Inclusão interna", titulo: "Novo membro", enviar: "Salvar membro" },
  EDICAO: { rotulo: "Atualização", titulo: "Editar membro", enviar: "Salvar membro" },
  CORRECAO: { rotulo: "Conferência pastoral", titulo: "Corrigir solicitação", enviar: "Salvar correção" },
};

export function MembroFormulario({
  origem, dados, erros, duplicados, dataIngresso, observacao, salvando,
  aoAlterarDados, aoAlterarIngresso, aoAlterarObservacao,
  aoEnviar, aoConfirmarDuplicidade, aoCancelar,
}: {
  origem: OrigemFormulario;
  dados: DadosMembro;
  erros: ErrosValidacao;
  duplicados: ErroDaApi["duplicados"];
  dataIngresso: string;
  observacao: string;
  salvando: boolean;
  aoAlterarDados: (dados: DadosMembro) => void;
  aoAlterarIngresso: (valor: string) => void;
  aoAlterarObservacao: (valor: string) => void;
  aoEnviar: () => void;
  aoConfirmarDuplicidade: () => void;
  aoCancelar: () => void;
}) {
  const texto = textos[origem];
  const correcao = origem === "CORRECAO";

  return (
    <form className="cartao-gestao-membro"
      onSubmit={(evento) => { evento.preventDefault(); aoEnviar(); }}>
      <TopoCartao rotulo={texto.rotulo} titulo={texto.titulo}>
        <button type="button" className="botao-secundario" onClick={aoCancelar}>Cancelar</button>
      </TopoCartao>

      <CamposMembro dados={dados} alterar={aoAlterarDados} erros={erros} interno={!correcao} />

      {!correcao && (
        <CamposPastorais dataIngresso={dataIngresso} observacao={observacao}
          aoAlterarIngresso={aoAlterarIngresso} aoAlterarObservacao={aoAlterarObservacao} />
      )}

      <div className="acoes-cartao-membro">
        <button type="submit" disabled={salvando}>
          {salvando ? "Salvando…" : texto.enviar}
        </button>
      </div>

      {duplicados.length > 0 && (
        <div className="aviso-duplicidade">
          <strong>Confira antes de criar outro cadastro</strong>
          {duplicados.map((item) => (
            <p key={item.id}>
              {item.nome} {item.sobrenome} · {rotuloSituacaoMembro(item.situacao)}
            </p>
          ))}
          <button type="button" disabled={salvando} onClick={aoConfirmarDuplicidade}>
            Confirmar cadastro separado
          </button>
        </div>
      )}
    </form>
  );
}
