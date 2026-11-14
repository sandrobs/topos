import { CampoFormulario } from "../CampoFormulario";
import {
  formatarDataHora, formatarDataIso, formatarTelefoneBrasileiro,
  rotuloAcaoHistorico,
} from "../formatadores";
import { funcoesMembro } from "../CadastroMembro";
import type { DadosMembro } from "../api";

/** Blocos usados tanto no detalhe do membro quanto no da solicitação. */

export function ResumoDados({ dados }: { dados: DadosMembro }) {
  const endereco = [dados.logradouro, dados.numero, dados.bairro,
    dados.complemento, dados.cep, dados.cidade, dados.estado].filter(Boolean).join(", ");

  return (
    <dl className="resumo-dados-membro">
      <div><dt>Função na igreja</dt>
        <dd>{funcoesMembro.find((item) => item.codigo === dados.funcao)?.nome ?? "Membro"}</dd></div>
      <div><dt>WhatsApp</dt>
        <dd>{dados.whatsapp ? formatarTelefoneBrasileiro(dados.whatsapp) : "Não informado"}</dd></div>
      <div><dt>E-mail</dt><dd>{dados.email || "Não informado"}</dd></div>
      <div><dt>Nascimento</dt><dd>{formatarDataIso(dados.dataNascimento) || "Não informado"}</dd></div>
      <div><dt>Endereço</dt><dd>{endereco || "Não informado"}</dd></div>
      <div><dt>Batismo</dt><dd>
        {dados.situacaoBatismo === "BATIZADO" ? "Batizado"
          : dados.situacaoBatismo === "NAO_BATIZADO" ? "Não batizado" : "Não informado"}
        {dados.dataBatismo ? ` · ${formatarDataIso(dados.dataBatismo)}` : ""}
      </dd></div>
      {dados.ehMenor && (
        <div><dt>Responsável legal</dt><dd>
          {dados.nomeResponsavelLegal} · {dados.vinculoResponsavelLegal}
          {dados.whatsappResponsavelLegal
            ? ` · ${formatarTelefoneBrasileiro(dados.whatsappResponsavelLegal)}` : ""}
        </dd></div>
      )}
    </dl>
  );
}

export type ItemHistorico = {
  acao: string;
  descricao: string | null;
  criadoEm: string;
  nomeAutor: string;
};

export function Historico({ itens }: { itens: ItemHistorico[] }) {
  return (
    <div className="historico-membro">
      <h3>Histórico</h3>
      {itens.map((item, indice) => (
        <p key={`${item.criadoEm}-${indice}`}>
          <strong>{rotuloAcaoHistorico(item.acao)}</strong> · {item.nomeAutor}
          {" · "}{formatarDataHora(item.criadoEm)}
          {item.descricao && <small>{item.descricao}</small>}
        </p>
      ))}
    </div>
  );
}

/** Data de ingresso e observação pastoral, pedidas no formulário e na aprovação. */
export function CamposPastorais({
  dataIngresso, observacao, aoAlterarIngresso, aoAlterarObservacao,
}: {
  dataIngresso: string;
  observacao: string;
  aoAlterarIngresso: (valor: string) => void;
  aoAlterarObservacao: (valor: string) => void;
}) {
  return (
    <div className="grade-campos-membro">
      <CampoFormulario id="data-ingresso" rotulo="Data de ingresso (opcional)"
        tipo="date" valor={dataIngresso} aoAlterar={aoAlterarIngresso} />
      <CampoFormulario id="observacao-pastoral" rotulo="Observação pastoral (interna)">
        {(atributos) => (
          <textarea {...atributos} value={observacao} maxLength={2000}
            onChange={(evento) => aoAlterarObservacao(evento.target.value)} />
        )}
      </CampoFormulario>
    </div>
  );
}

/**
 * Cabeçalho dos cartões de membro, no mesmo desenho do cabeçalho das seções:
 * rótulo e título à esquerda, ações à direita.
 */
export function TopoCartao({
  rotulo, titulo, children,
}: {
  rotulo: string;
  titulo: string;
  children?: React.ReactNode;
}) {
  return (
    <div className="topo-cartao-membro">
      <div>
        <p className="rotulo-painel">{rotulo}</p>
        <h2>{titulo}</h2>
      </div>
      {children && <div className="acoes-painel">{children}</div>}
    </div>
  );
}
