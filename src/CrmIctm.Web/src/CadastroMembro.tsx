import { useEffect, useRef, useState, type FormEvent } from "react";
import {
  ErroDaApi, enviarSolicitacaoMembro, obterFormularioMembro,
  type DadosMembro, type ErrosValidacao, type IgrejaPublica,
} from "./api";
import { CampoFormulario } from "./CampoFormulario";
import { focarPrimeiroErro } from "./foco";
import { formatarCep, formatarEndereco, formatarTelefoneBrasileiro } from "./formatadores";
import { DialogoAvisoPrivacidade } from "./DialogoAvisoPrivacidade";
import { LogoIgreja } from "./LogoIgreja";

const unidadesFederativas = [
  "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
  "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO",
];

export const dadosMembroVazios: DadosMembro = {
  funcao: "MEMBRO",
  nome: "", sobrenome: "", whatsapp: "", email: "", dataNascimento: null,
  logradouro: "", numero: "", bairro: "", complemento: "", cep: "",
  cidade: "", estado: "", situacaoBatismo: "NAO_INFORMADO", dataBatismo: null,
  ehMenor: false, nomeResponsavelLegal: "", whatsappResponsavelLegal: "",
  vinculoResponsavelLegal: "",
};

export const funcoesMembro = [
  { codigo: "MEMBRO", nome: "Membro" }, { codigo: "DIACONO", nome: "Diácono" },
  { codigo: "PRESBITERO", nome: "Presbítero" }, { codigo: "PASTOR", nome: "Pastor" },
] as const;

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
    const preenchimentoAutomatico: Partial<Record<keyof DadosMembro, string>> = {
      nome: "given-name", sobrenome: "family-name", whatsapp: "tel", email: "email",
      dataNascimento: "bday", logradouro: "address-line1", complemento: "address-line2",
      cep: "postal-code", cidade: "address-level2", estado: "address-level1",
    };
    const limites: Partial<Record<keyof DadosMembro, number>> = {
      nome: 100, sobrenome: 100, whatsapp: 16, email: 254, logradouro: 150,
      numero: 20, bairro: 100, complemento: 100, cep: 9, cidade: 100,
      nomeResponsavelLegal: 150, whatsappResponsavelLegal: 16, vinculoResponsavelLegal: 60,
    };
    return (
      <CampoFormulario
        key={id}
        id={`membro-${id}`}
        rotulo={titulo}
        obrigatorio={obrigatorio}
        erro={erros[id]?.[0]}
        tipo={tipo}
        valor={typeof valor === "string" ? valor : ""}
        autoComplete={preenchimentoAutomatico[id]}
        maxLength={limites[id]}
        placeholder={dica}
        inputMode={id === "cep" ? "numeric" : tipo === "tel" ? "tel" : undefined}
        aoAlterar={(texto) => atualizar(id, (
          id === "whatsapp" || id === "whatsappResponsavelLegal"
            ? formatarTelefoneBrasileiro(texto)
            : id === "cep" ? formatarCep(texto)
            : tipo === "date" ? texto || null : texto
        ) as DadosMembro[typeof id])}
      />
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

      <fieldset className="grupo-campos-membro">
        <legend>Função na igreja <em>*</em></legend>
        <div className="opcoes-membro">
          {funcoesMembro.map((funcao) => <label key={funcao.codigo}
            className={`opcao-membro${dados.funcao === funcao.codigo ? " selecionada" : ""}`}>
            <input className="controle-escondido" type="radio" name="funcao" required
              value={funcao.codigo} checked={dados.funcao === funcao.codigo}
              onChange={() => atualizar("funcao", funcao.codigo)} />
            <span className="marcador-opcao-membro" aria-hidden="true">✓</span>{funcao.nome}
          </label>)}
        </div>
        {erros.funcao?.[0] && <small className="erro-campo">{erros.funcao[0]}</small>}
      </fieldset>

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
          <CampoFormulario id="membro-estado" rotulo="UF" erro={erros.estado?.[0]}>
            {(atributos) => (
              <select {...atributos} value={dados.estado ?? ""}
                onChange={(evento) => atualizar("estado", evento.target.value)}>
                <option value="">Selecione</option>
                {unidadesFederativas.map((uf) => <option key={uf} value={uf}>{uf}</option>)}
              </select>
            )}
          </CampoFormulario>
        </div>
      </fieldset>

      <fieldset className="grupo-campos-membro">
        <legend>Batismo <small>(opcional)</small></legend>
        <div className="grade-campos-membro">
          <div className="opcoes-membro opcoes-batismo">
            {([{ codigo: "NAO_INFORMADO", nome: "Informarei depois" },
              { codigo: "BATIZADO", nome: "Sou batizado(a)" },
              { codigo: "NAO_BATIZADO", nome: "Ainda não sou batizado(a)" }] as const)
              .map((opcao) => <label key={opcao.codigo}
                className={`opcao-membro${dados.situacaoBatismo === opcao.codigo ? " selecionada" : ""}`}>
                <input className="controle-escondido" type="radio" name="situacaoBatismo"
                  checked={dados.situacaoBatismo === opcao.codigo}
                  onChange={() => alterar({ ...dados, situacaoBatismo: opcao.codigo,
                    dataBatismo: opcao.codigo === "BATIZADO" ? dados.dataBatismo : null })} />
                <span className="marcador-opcao-membro" aria-hidden="true">✓</span>{opcao.nome}
              </label>)}
          </div>
          {dados.situacaoBatismo === "BATIZADO" && campo("dataBatismo", "Data do batismo", "date")}
        </div>
      </fieldset>
    </div>
  );
}

export function FormularioPublicoMembros({ identificador }: { identificador: string }) {
  const [igreja, setIgreja] = useState<IgrejaPublica | null>(null);
  const [dados, setDados] = useState<DadosMembro>({ ...dadosMembroVazios });
  const [ciencia, setCiencia] = useState(false);
  const [abrirAviso, setAbrirAviso] = useState(false);
  const [carregando, setCarregando] = useState(true);
  const [enviando, setEnviando] = useState(false);
  const [enviado, setEnviado] = useState(false);
  const [erro, setErro] = useState("");
  const [erros, setErros] = useState<ErrosValidacao>({});
  const formularioRef = useRef<HTMLFormElement>(null);
  const confirmacaoRef = useRef<HTMLHeadingElement>(null);

  useEffect(() => { if (enviado) confirmacaoRef.current?.focus(); }, [enviado]);
  useEffect(() => {
    if (Object.keys(erros).length) focarPrimeiroErro(formularioRef.current);
  }, [erros]);

  useEffect(() => {
    void obterFormularioMembro(identificador)
      .then(setIgreja)
      .catch(() => setErro("Este formulário não está disponível."))
      .finally(() => setCarregando(false));
  }, [identificador]);

  async function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (enviando || !igreja?.avisoPrivacidade) return;
    if (!ciencia) {
      setErro("Confirme a maioridade e a ciência do aviso de privacidade.");
      return;
    }
    setEnviando(true);
    setErro("");
    setErros({});
    try {
      await enviarSolicitacaoMembro(identificador, dados,
        igreja.avisoPrivacidade.versao, ciencia);
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
              <span aria-hidden="true">✓</span><h1 ref={confirmacaoRef} tabIndex={-1}>Cadastro recebido</h1>
              <p>Obrigado! A equipe da igreja analisará as informações. O envio ainda não confirma o cadastro como membro.</p>
            </div>
          ) : (
            <>
              <header className="cabecalho-formulario">
                <p className="rotulo">Cadastro de membros</p>
                <h1>Uma família no amor de Deus</h1>
                <p className="localidade">Vamos manter nossos laços. Informe seus dados para que a igreja possa acompanhar você.</p>
              </header>
              <div className="introducao"><strong className="introducao-destaque">É simples e leva poucos minutos.</strong>
                <p>Campos com * são obrigatórios. Endereço e informações de batismo são opcionais.</p></div>
              {!igreja.avisoPrivacidade && <div className="alerta" role="alert">
                O cadastro ainda não está disponível. Procure a equipe da igreja.
              </div>}
              {erro && igreja.avisoPrivacidade && <div className="alerta" role="alert">{erro}</div>}
              {/*
                * `noValidate` desliga só a bolha do navegador, não os atributos
                * `required`, que seguem valendo como semântica para leitor de
                * tela. A validação é a do servidor, que devolve erro por campo
                * com as mesmas chaves — e aí o efeito acima leva o foco até o
                * primeiro deles. Com a validação nativa no caminho, o envio era
                * bloqueado antes, e esse efeito nunca chegava a disparar.
                */}
              <form ref={formularioRef} className="formulario-membro" onSubmit={enviar}
                aria-busy={enviando} noValidate>
                <CamposMembro dados={dados} alterar={setDados} erros={erros} />
                {igreja.avisoPrivacidade && (
                  <div className="aviso-privacidade">
                    <div className="resumo-privacidade"><p>Seus dados serão usados para cadastro, contato e acompanhamento pela igreja.</p>
                    <button type="button" className="abrir-aviso-privacidade"
                      aria-haspopup="dialog" aria-expanded={abrirAviso} aria-controls="dialogo-aviso-privacidade"
                      onClick={() => setAbrirAviso(true)}>Segurança e privacidade →</button></div>
                    <label className={`confirmacao-privacidade${ciencia ? " selecionada" : ""}`}>
                      <input className="controle-escondido" type="checkbox" required checked={ciencia}
                      onChange={(evento) => setCiencia(evento.target.checked)} />
                      <span className="caixa-confirmacao" aria-hidden="true">✓</span><span>
                        Sou maior de 18 anos e autorizo esta igreja a usar os dados informados, incluindo informações religiosas,
                        para cadastro e acompanhamento de membros e contato pelo WhatsApp. Li o Aviso de Privacidade.
                      </span></label>
                    {(erros.avisoPrivacidade || erros.declaracaoMaioridade) && <small className="erro-campo">
                      {erros.avisoPrivacidade?.[0] || erros.declaracaoMaioridade?.[0]}</small>}
                  </div>
                )}
                <button type="submit" disabled={enviando || !igreja.avisoPrivacidade}>
                  {enviando ? "Enviando…" : "Enviar cadastro para análise"}
                </button>
              </form>
              {igreja.avisoPrivacidade && (
                <DialogoAvisoPrivacidade aberto={abrirAviso} nomeIgreja={igreja.nome}
                  texto={igreja.avisoPrivacidade.texto} aoFechar={() => setAbrirAviso(false)} />
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
