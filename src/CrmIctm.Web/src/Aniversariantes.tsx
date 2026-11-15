import { useEffect, useMemo, useState } from "react";
import {
  ErroDaApi,
  listarAniversariantes,
  type Aniversariante,
  type ContadoresAniversarios,
  type IgrejaInterna,
  type ListaAniversariantes,
  type PeriodoAniversario,
} from "./api";
import { soltarFocoDePonteiro } from "./foco";
import {
  formatarDiaExtenso,
  formatarIntervaloDeDias,
  formatarMesExtenso,
  formatarTelefoneBrasileiro,
  iniciais,
  nomeDoMes,
  rotuloFuncaoMembro,
} from "./formatadores";

/*
 * A mensagem é fixa nesta versão, por decisão registrada em docs/v4: deixá-la
 * configurável por igreja pediria uma coluna nova, e a V4 não faz migração.
 *
 * As três variáveis são o primeiro nome do membro, o nome da igreja e o nome
 * de quem está usando o sistema. A assinatura vai sem título: o sistema não
 * sabe tratar por "Pastor" quem está logado como Administrador.
 *
 * Há duas versões porque a tela mostra também semana e mês: dizer "hoje" num
 * aniversário que é sexta-feira estaria simplesmente errado.
 */
function mensagemDeParabens(
  pessoa: Aniversariante, igreja: string, usuario: string, ehHoje: boolean,
) {
  const aniversariante = primeiroNome(pessoa.nome);

  /*
   * Quando a conversa abre com o responsável legal, a mensagem fala com ele.
   * Cumprimentar "Paz do Senhor, Davi!" num número que é da mãe soaria como
   * mensagem entregue à pessoa errada.
   */
  if (pessoa.destino === "RESPONSAVEL") {
    const quem = pessoa.nomeResponsavel ? primeiroNome(pessoa.nomeResponsavel) : "você";
    const abertura = ehHoje
      ? `Hoje a ${igreja} celebra o aniversário de ${aniversariante}.`
      : `A ${igreja} celebra o aniversário de ${aniversariante}.`;
    return `Paz do Senhor, ${quem}! ${abertura} Que Deus continue abençoando a`
      + ` sua família. Feliz aniversário! — ${usuario}`;
  }

  const abertura = ehHoje
    ? `Hoje a ${igreja} celebra com você.`
    : `A ${igreja} celebra com você pelo seu aniversário.`;
  return `Paz do Senhor, ${aniversariante}! ${abertura} Que Deus te dê saúde, paz`
    + ` e alegria neste novo ano de vida. Feliz aniversário! — ${usuario}`;
}

/**
 * O número é guardado como +55DDNNNNNNNNN. O wa.me quer só os dígitos.
 *
 * No celular isso abre o aplicativo; no computador abre o WhatsApp Web, que
 * depende de telefone pareado. É limitação do WhatsApp, e a tela não tenta
 * contornar. Em nenhum dos dois o sistema envia: o texto chega escrito na
 * conversa e quem toca em enviar é a pessoa.
 */
function enderecoWhatsapp(numero: string, mensagem: string) {
  return `https://wa.me/${numero.replace(/\D/g, "")}?text=${encodeURIComponent(mensagem)}`;
}

function primeiroNome(nome: string) {
  return nome.trim().split(/\s+/)[0] || nome;
}

function IconeWhatsapp() {
  return (
    <svg className="icone-whatsapp" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
      <path d="M12 2a10 10 0 0 0-8.7 15l-1.2 4.3 4.4-1.2A10 10 0 1 0 12 2zm0 2a8 8 0 1 1-4.2 14.8l-.4-.2-2.3.6.6-2.2-.2-.4A8 8 0 0 1 12 4zm-3.1 4.3c-.2 0-.5.1-.7.3-.3.3-.7.8-.7 1.6 0 .9.6 1.8.7 1.9.1.2 1.3 2.1 3.2 2.9 1.6.7 2 .6 2.4.5.4 0 1.2-.5 1.4-1 .2-.5.2-1 .1-1.1 0-.1-.2-.2-.4-.3l-1.3-.6c-.2-.1-.3 0-.5.1l-.5.6c-.1.2-.3.2-.5.1-.2-.1-.9-.4-1.6-1-.6-.6-1-1.2-1.1-1.4-.1-.2 0-.3.1-.4l.4-.5c.1-.2.1-.3 0-.5l-.5-1.3c-.1-.3-.3-.3-.5-.4z" />
    </svg>
  );
}

function LinhaAniversario({ pessoa, ehHoje, nomeIgreja, nomeUsuario }: {
  pessoa: Aniversariante;
  ehHoje: boolean;
  nomeIgreja: string;
  nomeUsuario: string;
}) {
  const nomeCompleto = `${pessoa.nome} ${pessoa.sobrenome}`.trim();
  const anos = `${pessoa.idadeQueCompleta} ${pessoa.idadeQueCompleta === 1 ? "ano" : "anos"}`;
  const idade = ehHoje ? `Faz ${anos} hoje` : `Completa ${anos}`;

  const responsavel = pessoa.nomeResponsavel
    ? pessoa.vinculoResponsavel
      ? `${pessoa.nomeResponsavel} (${pessoa.vinculoResponsavel})`
      : pessoa.nomeResponsavel
    : "o responsável legal";

  const contato = pessoa.destino === "NENHUM"
    ? "Cadastro sem WhatsApp"
    : pessoa.destino === "RESPONSAVEL"
      ? `Responsável: ${responsavel} — ${formatarTelefoneBrasileiro(pessoa.whatsappDestino ?? "")}`
      : formatarTelefoneBrasileiro(pessoa.whatsappDestino ?? "");

  const paraResponsavel = pessoa.destino === "RESPONSAVEL";
  const rotulo = paraResponsavel
    ? `Falar com ${pessoa.nomeResponsavel ? primeiroNome(pessoa.nomeResponsavel) : "o responsável"}`
    : "Parabenizar";
  const nomeAcessivel = paraResponsavel
    ? `Falar pelo WhatsApp com ${responsavel}, de ${nomeCompleto}`
    : `Parabenizar ${nomeCompleto} pelo WhatsApp`;

  return (
    <div className="linha-aniversario">
      <span className="avatar-membro" aria-hidden="true">{iniciais(nomeCompleto)}</span>
      <div className="dados-aniversario">
        <strong>{nomeCompleto}</strong>
        <span className="idade-aniversario">{idade}</span>
        <small>{contato}</small>
        <span className="selos-aniversario">
          <span className="selo-membro funcao">{rotuloFuncaoMembro(pessoa.funcao)}</span>
          {/* Cadastrado como menor e já completando 18: o cadastro guarda
              responsável legal que deixou de fazer sentido. */}
          {pessoa.ehMenor && pessoa.idadeQueCompleta >= 18 && (
            <span className="selo-membro atencao">
              Completa {pessoa.idadeQueCompleta} anos: revise o cadastro
            </span>
          )}
        </span>
      </div>
      {pessoa.whatsappDestino ? (
        <a
          className="botao-principal"
          href={enderecoWhatsapp(
            pessoa.whatsappDestino,
            mensagemDeParabens(pessoa, nomeIgreja, nomeUsuario, ehHoje),
          )}
          target="_blank"
          rel="noopener noreferrer"
          aria-label={nomeAcessivel}
        >
          <IconeWhatsapp />
          {rotulo}
        </a>
      ) : (
        /* Sem conversa a abrir, mas a pessoa continua na lista: a igreja ainda
           pode celebrar de outro jeito, e o motivo fica escrito. */
        <p className="sem-whatsapp">Sem WhatsApp no cadastro</p>
      )}
    </div>
  );
}

function GrupoDeDia({ data, pessoas, hoje, nomeIgreja, nomeUsuario }: {
  data: string;
  pessoas: Aniversariante[];
  hoje: string;
  nomeIgreja: string;
  nomeUsuario: string;
}) {
  return (
    <div className="grupo-aniversarios">
      <p className="dia-aniversarios">
        {formatarDiaExtenso(data)}
        {data === hoje && <span className="selo-dia-hoje">Hoje</span>}
      </p>
      {pessoas.map((pessoa) => (
        <LinhaAniversario
          key={pessoa.id}
          pessoa={pessoa}
          ehHoje={pessoa.dataOcorrencia === hoje}
          nomeIgreja={nomeIgreja}
          nomeUsuario={nomeUsuario}
        />
      ))}
    </div>
  );
}

export function Aniversariantes({ igreja, nomeUsuario, contadores, aoIrParaMembros }: {
  igreja: IgrejaInterna;
  nomeUsuario: string;
  contadores: ContadoresAniversarios | null;
  aoIrParaMembros: () => void;
}) {
  const [periodo, setPeriodo] = useState<PeriodoAniversario>("HOJE");
  const [lista, setLista] = useState<ListaAniversariantes | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState("");
  const [mostrarPassados, setMostrarPassados] = useState(false);

  useEffect(() => {
    let ativo = true;
    setCarregando(true);
    setErro("");
    listarAniversariantes(igreja.id, periodo)
      .then((resposta) => { if (ativo) setLista(resposta); })
      .catch((falha: unknown) => {
        if (!ativo) return;
        setErro(falha instanceof ErroDaApi
          ? falha.message
          : "Não foi possível carregar os aniversariantes.");
      })
      .finally(() => { if (ativo) setCarregando(false); });
    return () => { ativo = false; };
  }, [igreja.id, periodo]);

  // A lista já chega ordenada por data de ocorrência, então o Map preserva a
  // ordem do calendário — inclusive na semana que vira o ano.
  const grupos = useMemo(() => {
    const mapa = new Map<string, Aniversariante[]>();
    for (const item of lista?.itens ?? []) {
      const existente = mapa.get(item.dataOcorrencia);
      if (existente) existente.push(item);
      else mapa.set(item.dataOcorrencia, [item]);
    }
    return [...mapa].map(([data, pessoas]) => ({ data, pessoas }));
  }, [lista]);

  const hoje = lista?.hoje ?? "";
  const ehMes = periodo === "MES";
  const passados = ehMes ? grupos.filter((grupo) => grupo.data < hoje) : [];
  const adiante = ehMes ? grupos.filter((grupo) => grupo.data >= hoje) : grupos;
  const quantosPassados = passados.reduce((soma, grupo) => soma + grupo.pessoas.length, 0);

  const abas: Array<{ codigo: PeriodoAniversario; rotulo: string; total?: number }> = [
    { codigo: "HOJE", rotulo: "Hoje", total: contadores?.hoje },
    /* "Semana" e não "Esta semana": com três abas e um contador em cada, a
       versão longa estourava a largura a 375 px. */
    { codigo: "SEMANA", rotulo: "Semana", total: contadores?.semana },
    { codigo: "MES", rotulo: lista ? nomeDoMes(lista.hoje) : "No mês", total: contadores?.mes },
  ];

  /* Na aba Hoje o cabeçalho do dia já diz a data; repeti-la acima seria a mesma
     linha duas vezes. */
  const recorte = !lista || periodo === "HOJE"
    ? ""
    : periodo === "SEMANA"
      ? `Semana de ${formatarIntervaloDeDias(lista.inicio, lista.fim)}`
      : formatarMesExtenso(lista.hoje);

  return (
    <>
      <section className="apresentacao-painel">
        <div>
          <p className="rotulo-painel">Cuidado pastoral</p>
          <h1>Aniversariantes</h1>
          <p>
            Quem a igreja pode celebrar. Abrir a conversa deixa a mensagem
            escrita no WhatsApp — o envio continua sendo seu.
          </p>
        </div>
        <div className="lado-painel">
          <div className="resumo-painel">
            <div><strong>{contadores?.hoje ?? "—"}</strong><span>Hoje</span></div>
            <div><strong>{contadores?.semana ?? "—"}</strong><span>Semana</span></div>
            <div><strong>{contadores?.mes ?? "—"}</strong><span>No mês</span></div>
          </div>
        </div>
      </section>

      <div className="abas-membros" role="tablist" aria-label="Período">
        {abas.map((aba) => (
          <button
            key={aba.codigo}
            type="button"
            role="tab"
            aria-selected={periodo === aba.codigo}
            className={periodo === aba.codigo ? "ativo" : ""}
            onClick={(evento) => {
              setPeriodo(aba.codigo);
              setMostrarPassados(false);
              soltarFocoDePonteiro(evento);
            }}
          >
            {aba.rotulo}
            {aba.total !== undefined && aba.total > 0 && <span>{aba.total}</span>}
          </button>
        ))}
      </div>

      {erro && <div className="alerta" role="alert">{erro}</div>}

      {recorte && <p className="recorte-aniversarios">{recorte}</p>}

      <div className="lista-membros" aria-busy={carregando}>
        <p className="estado-lista" role="status" aria-live="polite">
          {carregando ? "Carregando aniversariantes…" : ""}
        </p>

        {!carregando && !erro && grupos.length === 0 && (
          <div className="vazio-membros">
            <p>
              {periodo === "HOJE"
                ? "Ninguém da igreja faz aniversário hoje."
                : "Nenhum aniversário neste período."}
            </p>
            {periodo === "HOJE" && contadores && contadores.semana > 0 && (
              <button
                type="button"
                className="link-aniversarios"
                onClick={(evento) => { setPeriodo("SEMANA"); soltarFocoDePonteiro(evento); }}
              >
                Ver os {contadores.semana} desta semana
              </button>
            )}
          </div>
        )}

        {ehMes && quantosPassados > 0 && (
          <>
            <button
              type="button"
              className="passados-aniversarios"
              aria-expanded={mostrarPassados}
              onClick={(evento) => {
                setMostrarPassados((atual) => !atual);
                soltarFocoDePonteiro(evento);
              }}
            >
              <span>
                {formatarIntervaloDeDias(lista!.inicio, passados[passados.length - 1]!.data)}
                {" · "}
                {quantosPassados === 1
                  ? "1 aniversário já passou"
                  : `${quantosPassados} aniversários já passaram`}
              </span>
              <span aria-hidden="true">{mostrarPassados ? "Ocultar" : "Mostrar"}</span>
            </button>
            {mostrarPassados && passados.map((grupo) => (
              <GrupoDeDia
                key={grupo.data}
                data={grupo.data}
                pessoas={grupo.pessoas}
                hoje={hoje}
                nomeIgreja={igreja.nome}
                nomeUsuario={nomeUsuario}
              />
            ))}
          </>
        )}

        {adiante.map((grupo) => (
          <GrupoDeDia
            key={grupo.data}
            data={grupo.data}
            pessoas={grupo.pessoas}
            hoje={hoje}
            nomeIgreja={igreja.nome}
            nomeUsuario={nomeUsuario}
          />
        ))}
      </div>

      {/*
        * Sem este aviso a falha seria silenciosa: a data de nascimento é
        * opcional no cadastro, e quem está sem ela nunca aparece na lista —
        * ela só fica mais curta, sem erro e sem explicação.
        */}
      {contadores && contadores.semDataNascimento > 0 && (
        <div className="aviso-sem-data">
          <p>
            <strong>
              {contadores.semDataNascimento === 1
                ? "1 membro ativo está sem data de nascimento"
                : `${contadores.semDataNascimento} membros ativos estão sem data de nascimento`}
            </strong>
            {" "}e por isso não aparecem aqui.
          </p>
          <button type="button" className="link-aniversarios" onClick={aoIrParaMembros}>
            Ver e completar o cadastro
          </button>
        </div>
      )}
    </>
  );
}
