import {
  Suspense, lazy, useEffect, useMemo, useRef, useState,
  type FormEvent, type ReactNode,
} from "react";
import { DialogoAvisoPrivacidade } from "./DialogoAvisoPrivacidade";
import {
  ErroDaApi,
  enviarAtendimento,
  obterIgreja,
  type ErrosValidacao,
  type IgrejaPublica,
} from "./api";
import { LogoIgreja } from "./LogoIgreja";
import { FormularioPublicoMembros } from "./CadastroMembro";
import { CampoFormulario } from "./CampoFormulario";
import { focarPrimeiroErro } from "./foco";
import { tituloPublico, useTitulo } from "./titulo";
import { formatarEndereco, formatarTelefoneBrasileiro } from "./formatadores";

/*
 * As rotas internas só carregam quando alguém as acessa. Sem isto, quem escaneia
 * o QR Code na porta da igreja baixa o painel, o kanban e o @dnd-kit para
 * preencher dois campos.
 */
const PaginaLogin = lazy(() =>
  import("./Interno").then((modulo) => ({ default: modulo.PaginaLogin })));
const PainelAtendimentos = lazy(() =>
  import("./Interno").then((modulo) => ({ default: modulo.PainelAtendimentos })));
const PaginaQrCode = lazy(() =>
  import("./QrCode").then((modulo) => ({ default: modulo.PaginaQrCode })));
const PaginaQrCodeMembros = lazy(() =>
  import("./QrCodeMembros").then((modulo) => ({ default: modulo.PaginaQrCodeMembros })));

type EstadoFormulario = {
  nome: string;
  whatsapp: string;
  querConhecerIgreja: boolean;
  querConversaOracao: boolean;
  avisoReconhecido: boolean;
};

const formularioInicial: EstadoFormulario = {
  nome: "",
  whatsapp: "",
  querConhecerIgreja: false,
  querConversaOracao: false,
  avisoReconhecido: false,
};

function obterIdentificadorPublico(): string | null {
  const partes = window.location.pathname.split("/").filter(Boolean);
  return partes[0] === "visita" && partes[1] ? partes[1] : null;
}

export function App() {
  const identificador = useMemo(obterIdentificadorPublico, []);
  const caminho = window.location.pathname;
  const partesQrCode = caminho.split("/").filter(Boolean);

  const interna = (conteudo: ReactNode) => (
    <Suspense fallback={<CarregandoRota />}>{conteudo}</Suspense>
  );

  if (caminho === "/entrar") {
    return interna(<PaginaLogin />);
  }

  if (caminho === "/painel") {
    return interna(<PainelAtendimentos />);
  }

  if (partesQrCode[0] === "qrcode" && partesQrCode[1]) {
    return interna(<PaginaQrCode igrejaId={partesQrCode[1]} />);
  }

  if (partesQrCode[0] === "qrcode-membros" && partesQrCode[1]) {
    return interna(<PaginaQrCodeMembros igrejaId={partesQrCode[1]} />);
  }

  if (partesQrCode[0] === "membros" && partesQrCode[1] === "cadastro" && partesQrCode[2]) {
    return <FormularioPublicoMembros identificador={partesQrCode[2]} />;
  }

  if (!identificador) {
    return <PaginaInicial />;
  }

  return <FormularioPublico identificador={identificador} />;
}

function FormularioPublico({ identificador }: { identificador: string }) {
  const [igreja, setIgreja] = useState<IgrejaPublica | null>(null);
  const [formulario, setFormulario] = useState(formularioInicial);
  const [carregando, setCarregando] = useState(true);
  const [enviando, setEnviando] = useState(false);
  const [erroPagina, setErroPagina] = useState("");
  const [erros, setErros] = useState<ErrosValidacao>({});
  const formularioRef = useRef<HTMLFormElement>(null);
  useTitulo(tituloPublico("Boas-vindas", igreja?.nome));

  // Só depois do commit do React os aria-invalid existem no DOM.
  useEffect(() => {
    if (Object.keys(erros).length) focarPrimeiroErro(formularioRef.current);
  }, [erros]);
  const [confirmacao, setConfirmacao] = useState("");
  const [avisoPrivacidadeAberto, setAvisoPrivacidadeAberto] = useState(false);

  useEffect(() => {
    const carregar = async () => {
      try {
        setIgreja(await obterIgreja(identificador));
      } catch (erro) {
        setErroPagina(erro instanceof Error ? erro.message : "Esta página não está disponível.");
      } finally {
        setCarregando(false);
      }
    };

    void carregar();
  }, [identificador]);

  async function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (!igreja?.avisoPrivacidade) {
      return;
    }

    setEnviando(true);
    setErros({});
    setErroPagina("");

    try {
      const mensagem = await enviarAtendimento(identificador, {
        nome: formulario.nome,
        whatsapp: formulario.whatsapp,
        querConhecerIgreja: formulario.querConhecerIgreja,
        querConversaOracao: formulario.querConversaOracao,
        avisoPrivacidadeReconhecido: formulario.avisoReconhecido,
        avisoPrivacidadeVersao: igreja.avisoPrivacidade.versao,
      });
      setConfirmacao(mensagem);
    } catch (erro) {
      if (erro instanceof ErroDaApi) {
        setErros(erro.erros);
        setErroPagina(erro.message);
      } else {
        setErroPagina("Não foi possível enviar seus dados. Verifique sua conexão e tente novamente.");
      }
    } finally {
      setEnviando(false);
    }
  }

  if (carregando) {
    return <MensagemDePagina titulo="Carregando…" texto="Estamos preparando o formulário." />;
  }

  if (!igreja) {
    return <MensagemDePagina titulo="Página indisponível" texto={erroPagina} />;
  }

  if (confirmacao) {
    return <MensagemDePagina titulo="Obrigado pelo contato!" texto={confirmacao} destaque />;
  }

  const avisoDisponivel = igreja.avisoPrivacidade !== null;

  return (
    <main className="pagina-formulario">
      <div className="estrutura-formulario">
        <header className="topo-publico">
          <div className="identidade-publica">
            <LogoIgreja />
            <div>
              <strong>{igreja.nome}</strong>
              <small>{igreja.cidade} · {igreja.estado}</small>
            </div>
          </div>
          <span className="selo-boas-vindas">Formulário de boas-vindas</span>
        </header>

        <section className="cartao-formulario" aria-labelledby="titulo-formulario">
          <header className="cabecalho-formulario">
            <p className="rotulo">Queremos conhecer você</p>
            <h1 id="titulo-formulario">Seja muito bem-vindo!</h1>
            <p className="localidade">É uma alegria receber sua visita.</p>
          </header>

          <div className="introducao">
            <span className="introducao-destaque">Vamos manter contato?</span>
            <p>Deixe seus dados e nossa equipe falará com você pelo WhatsApp.</p>
          </div>

          {!avisoDisponivel && (
            <div className="alerta" role="alert">
              O cadastro ainda não está disponível. Procure a equipe da igreja.
            </div>
          )}

          {erroPagina && avisoDisponivel && (
            <div className="alerta" role="alert">{erroPagina}</div>
          )}

          <form ref={formularioRef} className="formulario-visitante" onSubmit={enviar} noValidate>
          <CampoFormulario
            id="nome"
            rotulo="Seu nome"
            obrigatorio
            autoComplete="name"
            valor={formulario.nome}
            erro={erros.nome?.[0]}
            aoAlterar={(nome) => setFormulario((atual) => ({ ...atual, nome }))}
          />
          <CampoFormulario
            id="whatsapp"
            rotulo="WhatsApp com DDD"
            obrigatorio
            tipo="tel"
            inputMode="tel"
            autoComplete="tel"
            placeholder="(11) 98765-4321"
            valor={formulario.whatsapp}
            erro={erros.whatsapp?.[0]}
            aoAlterar={(whatsapp) => setFormulario((atual) => ({
              ...atual,
              whatsapp: formatarTelefoneBrasileiro(whatsapp),
            }))}
          />

          <fieldset className="grupo-interesses">
            <legend>Como podemos acolher você?</legend>
            <p className="ajuda-campo">Você pode marcar mais de uma opção.</p>
            <OpcaoMarcavel
              icone="igreja"
              titulo="Conhecer melhor a igreja"
              descricao="Quero saber mais sobre a comunidade e suas atividades."
              selecionada={formulario.querConhecerIgreja}
              aoAlterar={(querConhecerIgreja) => setFormulario((atual) => ({
                ...atual,
                querConhecerIgreja,
              }))}
            />
            <OpcaoMarcavel
              icone="coracao"
              titulo="Conversar e receber oração"
              descricao="Gostaria que alguém da equipe entrasse em contato comigo."
              selecionada={formulario.querConversaOracao}
              aoAlterar={(querConversaOracao) => setFormulario((atual) => ({
                ...atual,
                querConversaOracao,
              }))}
            />
            <ErroCampo mensagem={erros.interesses?.[0]} />
          </fieldset>

          {igreja.avisoPrivacidade && (
            <div className="aviso-privacidade">
              <div className="resumo-privacidade">
                <p>Usaremos seus dados somente para contato e acolhimento.</p>
                <button
                  type="button"
                  className="abrir-aviso-privacidade"
                  aria-haspopup="dialog"
                  aria-expanded={avisoPrivacidadeAberto}
                  aria-controls="dialogo-aviso-privacidade"
                  onClick={() => setAvisoPrivacidadeAberto(true)}
                >
                  Segurança e privacidade
                  <span aria-hidden="true">→</span>
                </button>
              </div>
              <label className={`confirmacao-privacidade${formulario.avisoReconhecido ? " selecionada" : ""}`}>
                <input
                  id="confirmacao-privacidade"
                  className="controle-escondido"
                  type="checkbox"
                  checked={formulario.avisoReconhecido}
                  aria-invalid={Boolean(erros.avisoPrivacidade?.[0])}
                  aria-describedby={erros.avisoPrivacidade?.[0] ? "erro-aviso-privacidade" : undefined}
                  onChange={(evento) => setFormulario((atual) => ({
                    ...atual,
                    avisoReconhecido: evento.target.checked,
                  }))}
                />
                <span className="caixa-confirmacao" aria-hidden="true">✓</span>
                <span>
                  Confirmo que sou maior de 18 anos ou responsável legal e autorizo esta igreja a usar os
                  dados informados para entrar em contato comigo pelo WhatsApp e realizar meu acolhimento.
                  Li o Aviso de Privacidade.
                </span>
              </label>
              <ErroCampo id="erro-aviso-privacidade" mensagem={erros.avisoPrivacidade?.[0]} />

              <DialogoAvisoPrivacidade
                aberto={avisoPrivacidadeAberto}
                nomeIgreja={igreja.nome}
                texto={igreja.avisoPrivacidade.texto}
                aoFechar={() => setAvisoPrivacidadeAberto(false)}
              />
            </div>
          )}

            <button type="submit" disabled={enviando || !avisoDisponivel}>
              <span>{enviando ? "Enviando…" : "Enviar meus dados"}</span>
              {!enviando && <span className="seta-botao" aria-hidden="true">→</span>}
            </button>
          </form>
        </section>

        <footer className="rodape-publico">
          <span className="icone-seguranca" aria-hidden="true">✓</span>
          <p>
            <strong>Seus dados estão protegidos.</strong>{" "}
            Usaremos suas informações apenas para o contato da igreja.
            <small className="endereco-rodape-publico">{formatarEndereco(igreja)}</small>
          </p>
        </footer>
      </div>
    </main>
  );
}


function OpcaoMarcavel({
  icone,
  titulo,
  descricao,
  selecionada,
  aoAlterar,
}: {
  icone: "igreja" | "coracao";
  titulo: string;
  descricao: string;
  selecionada: boolean;
  aoAlterar: (selecionada: boolean) => void;
}) {
  return (
    <label className={`opcao-marcavel${selecionada ? " selecionada" : ""}`}>
      <input
        className="controle-escondido"
        type="checkbox"
        checked={selecionada}
        onChange={(evento) => aoAlterar(evento.target.checked)}
      />
      <span className="opcao-icone" aria-hidden="true">
        {icone === "igreja" ? <IconeIgreja /> : <IconeCoracao />}
      </span>
      <span className="opcao-texto">
        <strong>{titulo}</strong>
        <small>{descricao}</small>
      </span>
      <span className="indicador-selecao" aria-hidden="true">✓</span>
    </label>
  );
}

function IconeIgreja() {
  return (
    <svg viewBox="0 0 24 24" role="img">
      <path d="M12 2v5M9.5 4.5h5M5 11l7-4 7 4v9H5zM9 20v-5h6v5" />
    </svg>
  );
}

function IconeCoracao() {
  return (
    <svg viewBox="0 0 24 24" role="img">
      <path d="M20.8 5.8a5.5 5.5 0 0 0-7.8 0L12 6.9l-1.1-1.1a5.5 5.5 0 0 0-7.8 7.8L12 22l8.8-8.4a5.5 5.5 0 0 0 0-7.8Z" />
    </svg>
  );
}

function ErroCampo({ id, mensagem }: { id?: string; mensagem?: string }) {
  return mensagem ? <p id={id} className="erro-campo">{mensagem}</p> : null;
}

function MensagemDePagina({
  titulo,
  texto,
  destaque = false,
}: {
  titulo: string;
  texto: string;
  destaque?: boolean;
}) {
  return (
    <main className="pagina-mensagem">
      <section className={destaque ? "mensagem mensagem-sucesso" : "mensagem"}>
        <LogoIgreja />
        <h1>{titulo}</h1>
        <p>{texto}</p>
      </section>
    </main>
  );
}

/*
 * Espera pela rota interna. Vinda do cache, ela chega em poucos milissegundos:
 * mostrar a mensagem de imediato faria piscar um layout inteiro a cada
 * navegação. A classe `aguardando-rota` só a revela se a espera passar de
 * 400 ms, que é quando o usuário realmente precisa saber que algo está vindo.
 */
function CarregandoRota() {
  return (
    <main className="pagina-mensagem aguardando-rota">
      <section className="mensagem">
        <LogoIgreja />
        <h1>Carregando…</h1>
        <p>Estamos abrindo esta página.</p>
      </section>
    </main>
  );
}

function PaginaInicial() {
  return (
    <main className="pagina-mensagem">
      <section className="mensagem">
        <LogoIgreja />
        <h1>CRM de visitantes</h1>
        <p>Acesse o endereço presente no QR Code da sua igreja.</p>
        <a className="link-acao" href="/entrar">Acessar painel</a>
      </section>
    </main>
  );
}
