import { useEffect, useState } from "react";
import { ErroDaApi, listarIgrejas, type IgrejaInterna } from "./api";
import { enderecoPainelDaIgreja, salvarIgrejaSelecionada } from "./contextoIgreja";
import { LogoIgreja } from "./LogoIgreja";
import { formatarEndereco } from "./formatadores";

export function PaginaQrCode({ igrejaId }: { igrejaId: string }) {
  const [igreja, setIgreja] = useState<IgrejaInterna | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState("");
  const dispositivoIpad = ehIpad();

  useEffect(() => {
    const carregar = async () => {
      salvarIgrejaSelecionada(igrejaId);
      try {
        const igrejas = await listarIgrejas();
        const igrejaSelecionada = igrejas.find((item) => item.id === igrejaId && item.ativa);
        if (!igrejaSelecionada) {
          setErro("Não foi possível encontrar a igreja selecionada.");
          return;
        }
        setIgreja(igrejaSelecionada);
      } catch (falha) {
        if (falha instanceof ErroDaApi && falha.status === 401) {
          window.location.replace("/entrar");
          return;
        }

        setErro(falha instanceof Error ? falha.message : "Não foi possível preparar o cartaz.");
      } finally {
        setCarregando(false);
      }
    };

    void carregar();
  }, [igrejaId]);

  if (carregando) {
    return <EstadoCartaz titulo="Preparando o convite…" />;
  }

  if (!igreja) {
    return <EstadoCartaz titulo="Cartaz indisponível" texto={erro} />;
  }

  return (
    <main className={`pagina-cartaz${dispositivoIpad ? " pagina-cartaz-ipad" : ""}`}>
      <nav className="acoes-cartaz" aria-label="Ações do cartaz">
        <a href={enderecoPainelDaIgreja(igreja.id)}>← Voltar ao painel</a>
        <button type="button" onClick={() => window.print()}>Imprimir em A4</button>
      </nav>

      <article className="cartaz-a4" aria-label={`Convite de boas-vindas da ${igreja.nome}`}>
        <header className="cabecalho-cartaz">
          <LogoIgreja className="logo-cartaz" />
          <div>
            <span>Seja bem-vindo à</span>
            <strong>{igreja.nome}</strong>
            <small>{igreja.cidade} · {igreja.estado}</small>
          </div>
        </header>

        <section className="imagem-cartaz">
          <img
            src="/imagem-nave-igreja.png"
            alt="Altar e cruz da igreja de Três de Maio durante um momento de louvor"
          />
        </section>

        <section className="chamada-cartaz" aria-label="Convite e referência bíblica">
          <span>Você é nosso convidado</span>
          <strong>Ainda há lugar.</strong>
          <cite>Lucas 14:22</cite>
        </section>

        <section className="conteudo-cartaz">
          <div className="texto-cartaz">
            <p className="rotulo-cartaz">Vamos manter contato?</p>
            <h1>Queremos continuar essa conversa.</h1>
            <p className="descricao-cartaz">
              Escaneie o QR Code e deixe seu nome e WhatsApp. Nossa equipe entrará em contato
              para acolher você, apresentar melhor a igreja ou conversar e orar.
            </p>

            <ol className="passos-cartaz">
              <li><span>1</span>Abra a câmera do celular</li>
              <li><span>2</span>Aponte para o QR Code</li>
              <li><span>3</span>Toque no link e preencha</li>
            </ol>
          </div>

          <div className="area-qr-code">
            <div className="moldura-qr-code">
              <img
                src={`/api/igrejas/${igreja.id}/qrcode`}
                alt="QR Code para acessar o formulário de visitantes"
              />
            </div>
            <strong>Escaneie aqui</strong>
            <small>É rápido e seguro</small>
          </div>
        </section>

        <footer className="rodape-cartaz">
          <p><strong>Ficamos felizes com a sua visita.</strong><span>Esperamos falar com você em breve.</span></p>
          <span className="endereco-rodape-cartaz">{formatarEndereco(igreja)}</span>
        </footer>
      </article>
    </main>
  );
}

function ehIpad() {
  return /iPad/i.test(navigator.userAgent)
    || (navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1);
}

function EstadoCartaz({ titulo, texto }: { titulo: string; texto?: string }) {
  return (
    <main className="pagina-mensagem">
      <section className="mensagem">
        <LogoIgreja />
        <h1>{titulo}</h1>
        {texto && <p>{texto}</p>}
        <a className="link-acao" href="/painel">Voltar ao painel</a>
      </section>
    </main>
  );
}
