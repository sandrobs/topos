import { useEffect, useState, type FormEvent } from "react";
import {
  ErroDaApi, enviarSolicitacaoMembro, obterFormularioMembro,
  type DadosMembro, type ErrosValidacao, type IgrejaPublica,
} from "./api";
import { formatarEndereco, formatarTelefoneBrasileiro } from "./formatadores";
import { LogoIgreja } from "./LogoIgreja";

const unidadesFederativas = [
  "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
  "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO",
];

export const dadosMembroVazios: DadosMembro = {
  nome: "", sobrenome: "", whatsapp: "", email: "", dataNascimento: null,
  logradouro: "", numero: "", bairro: "", complemento: "", cep: "",
  cidade: "", estado: "", situacaoBatismo: "NAO_INFORMADO", dataBatismo: null,
  ehMenor: false, nomeResponsavelLegal: "", whatsappResponsavelLegal: "",
  vinculoResponsavelLegal: "",
};

type CamposProps = {
  dados: DadosMembro;
  alterar: (dados: DadosMembro) => void;
  erros?: ErrosValidacao;
  interno?: boolean;
};

export function CamposMembro({ dados, alterar, erros = {}, interno = false }: CamposProps) {
  function atualizar<K extends keyof DadosMembro>(campo: K, valor: DadosMembro[K]) {
    alterar({ ...dados, [campo]: valor });
  }

  function campo(id: keyof DadosMembro, titulo: string, tipo = "text",
    obrigatorio = false, dica?: string) {
    const valor = dados[id];
    return (
      <label className="campo-membro" key={id}>
        <span>{titulo}{obrigatorio && <em> *</em>}</span>
        <input
          type={tipo} value={typeof valor === "string" ? valor : ""}
          required={obrigatorio} placeholder={dica}
          aria-invalid={Boolean(erros[id]?.[0])}
          onChange={(evento) => atualizar(id, (
            id === "whatsapp" || id === "whatsappResponsavelLegal"
              ? formatarTelefoneBrasileiro(evento.target.value)
              : tipo === "date" ? evento.target.value || null : evento.target.value
          ) as DadosMembro[typeof id])}
        />
        {erros[id]?.[0] && <small className="erro-campo">{erros[id][0]}</small>}
      </label>
    );
  }

  return (
    <div className="campos-membro">
      {interno && (
        <label className="opcao-membro-menor">
          <input type="checkbox" checked={dados.ehMenor}
            onChange={(evento) => atualizar("ehMenor", evento.target.checked)} />
          Este cadastro é de menor de idade
        </label>
      )}
      <div className="grade-campos-membro">
        {campo("nome", "Nome", "text", true)}
        {campo("sobrenome", "Sobrenome", "text", true)}
        {campo("whatsapp", "WhatsApp com DDD", "tel", !dados.ehMenor, "(11) 98765-4321")}
        {campo("email", "E-mail", "email")}
        {campo("dataNascimento", "Data de nascimento", "date")}
      </div>

      {interno && dados.ehMenor && (
        <fieldset className="grupo-campos-membro">
          <legend>Responsável legal</legend>
          <div className="grade-campos-membro">
            {campo("nomeResponsavelLegal", "Nome do responsável", "text", true)}
            {campo("whatsappResponsavelLegal", "WhatsApp do responsável", "tel", true)}
            {campo("vinculoResponsavelLegal", "Vínculo", "text", true, "Ex.: mãe, pai, tutor")}
          </div>
        </fieldset>
      )}

      <fieldset className="grupo-campos-membro">
        <legend>Endereço <small>(opcional)</small></legend>
        <div className="grade-campos-membro">
          {campo("logradouro", "Rua ou avenida")}
          {campo("numero", "Número")}
          {campo("bairro", "Bairro")}
          {campo("complemento", "Complemento")}
          {campo("cep", "CEP", "text", false, "00000-000")}
          {campo("cidade", "Cidade")}
          <label className="campo-membro"><span>UF</span>
            <select value={dados.estado ?? ""}
              onChange={(evento) => atualizar("estado", evento.target.value)}>
              <option value="">Selecione</option>
              {unidadesFederativas.map((uf) => <option key={uf} value={uf}>{uf}</option>)}
            </select>
            {erros.estado?.[0] && <small className="erro-campo">{erros.estado[0]}</small>}
          </label>
        </div>
      </fieldset>

      <fieldset className="grupo-campos-membro">
        <legend>Batismo <small>(opcional)</small></legend>
        <div className="grade-campos-membro">
          <label className="campo-membro"><span>Situação</span>
            <select value={dados.situacaoBatismo}
              onChange={(evento) => alterar({ ...dados,
                situacaoBatismo: evento.target.value as DadosMembro["situacaoBatismo"],
                dataBatismo: evento.target.value === "BATIZADO" ? dados.dataBatismo : null })}>
              <option value="NAO_INFORMADO">Não informado</option>
              <option value="BATIZADO">Batizado</option>
              <option value="NAO_BATIZADO">Não batizado</option>
            </select>
          </label>
          {dados.situacaoBatismo === "BATIZADO" && campo("dataBatismo", "Data do batismo", "date")}
        </div>
      </fieldset>
    </div>
  );
}

export function FormularioPublicoMembros({ identificador }: { identificador: string }) {
  const [igreja, setIgreja] = useState<IgrejaPublica | null>(null);
  const [dados, setDados] = useState<DadosMembro>({ ...dadosMembroVazios });
  const [maioridade, setMaioridade] = useState(false);
  const [ciencia, setCiencia] = useState(false);
  const [abrirAviso, setAbrirAviso] = useState(false);
  const [carregando, setCarregando] = useState(true);
  const [enviando, setEnviando] = useState(false);
  const [enviado, setEnviado] = useState(false);
  const [erro, setErro] = useState("");
  const [erros, setErros] = useState<ErrosValidacao>({});

  useEffect(() => {
    void obterFormularioMembro(identificador)
      .then(setIgreja)
      .catch(() => setErro("Este formulário não está disponível."))
      .finally(() => setCarregando(false));
  }, [identificador]);

  async function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (!igreja?.avisoPrivacidade) return;
    if (!maioridade || !ciencia) {
      setErro("Confirme a maioridade e a ciência do aviso de privacidade.");
      return;
    }
    setEnviando(true);
    setErro("");
    setErros({});
    try {
      await enviarSolicitacaoMembro(identificador, dados,
        igreja.avisoPrivacidade.versao, maioridade);
      setEnviado(true);
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : "Não foi possível enviar o cadastro.");
      if (falha instanceof ErroDaApi) setErros(falha.erros);
    } finally {
      setEnviando(false);
    }
  }

  return (
    <main className="pagina-formulario pagina-formulario-membros">
      <div className="estrutura-formulario">
        <header className="topo-publico">
          <div className="identidade-publica"><LogoIgreja />
            <div><strong>{igreja?.nome ?? "Cadastro de membros"}</strong>
              <small>{igreja ? `${igreja.cidade} · ${igreja.estado}` : ""}</small></div>
          </div>
          <span className="selo-boas-vindas">Cadastro de membros</span>
        </header>
        <section className="cartao-formulario">
          {carregando ? <p>Preparando o formulário…</p> : !igreja ? (
            <div className="alerta" role="alert">{erro}</div>
          ) : enviado ? (
            <div className="confirmacao-membro">
              <span aria-hidden="true">✓</span><h1>Cadastro recebido</h1>
              <p>Obrigado! A equipe da igreja analisará as informações. O envio ainda não confirma o cadastro como membro.</p>
            </div>
          ) : (
            <>
              <header className="cabecalho-formulario">
                <p className="rotulo">Nossa comunidade</p>
                <h1>Atualize seus dados com a igreja</h1>
                <p className="localidade">Preencha apenas as informações que souber. Campos com * são obrigatórios.</p>
              </header>
              {!igreja.avisoPrivacidade && <div className="alerta" role="alert">
                O cadastro ainda não está disponível. Procure a equipe da igreja.
              </div>}
              {erro && igreja.avisoPrivacidade && <div className="alerta" role="alert">{erro}</div>}
              <form className="formulario-membro" onSubmit={enviar}>
                <CamposMembro dados={dados} alterar={setDados} erros={erros} />
                {igreja.avisoPrivacidade && (
                  <div className="aviso-membro">
                    <button type="button" className="abrir-aviso-privacidade"
                      onClick={() => setAbrirAviso(true)}>Leia o aviso de privacidade →</button>
                    <label><input type="checkbox" checked={maioridade}
                      onChange={(evento) => setMaioridade(evento.target.checked)} />
                      Confirmo que sou maior de 18 anos.</label>
                    <label><input type="checkbox" checked={ciencia}
                      onChange={(evento) => setCiencia(evento.target.checked)} />
                      Li o aviso de privacidade e estou ciente do uso dos meus dados para cadastro e gestão de membros desta igreja.</label>
                  </div>
                )}
                <button type="submit" disabled={enviando || !igreja.avisoPrivacidade}>
                  {enviando ? "Enviando…" : "Enviar cadastro para análise"}
                </button>
              </form>
              {abrirAviso && igreja.avisoPrivacidade && (
                <div className="sobreposicao sobreposicao-dialogo" role="presentation"
                  onMouseDown={(evento) => evento.target === evento.currentTarget && setAbrirAviso(false)}>
                  <section className="dialogo-conclusao aviso-membro-dialogo" role="dialog"
                    aria-modal="true" aria-label="Aviso de privacidade de membros">
                    <h2>Privacidade dos membros</h2>
                    <p>{igreja.avisoPrivacidade.texto}</p>
                    <button type="button" onClick={() => setAbrirAviso(false)}>Entendi</button>
                  </section>
                </div>
              )}
            </>
          )}
        </section>
        {igreja && <footer className="rodape-publico"><span className="icone-seguranca">✓</span>
          <p><strong>{igreja.nome}</strong><small className="endereco-rodape-publico">
            {formatarEndereco(igreja)}</small></p></footer>}
      </div>
    </main>
  );
}
