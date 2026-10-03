import { useEffect, useState } from "react";
import { ErroDaApi, listarIgrejas, type IgrejaInterna } from "./api";
import { enderecoPainelDaIgreja, salvarIgrejaSelecionada } from "./contextoIgreja";
import { LogoIgreja } from "./LogoIgreja";
import { formatarCep, formatarEndereco } from "./formatadores";

export function PaginaQrCode({ igrejaId, paraMembros = false }: { igrejaId: string; paraMembros?: boolean }) {
  const [igreja, setIgreja] = useState<IgrejaInterna | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState("");
  const enderecoRetorno = enderecoPainelDaIgreja(igrejaId, paraMembros ? "MEMBROS" : "ATENDIMENTOS");

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
    return <EstadoCartaz titulo="Preparando o convite…" enderecoRetorno={enderecoRetorno} />;
  }

  if (!igreja) {
    return <EstadoCartaz titulo="Cartaz indisponível" texto={erro} enderecoRetorno={enderecoRetorno} />;
  }

  const enderecoPdf = paraMembros ? `/api/membros/convite.pdf?igrejaId=${igreja.id}` : `/api/igrejas/${igreja.id}/convite.pdf`;
  const enderecoQr = paraMembros ? `/api/membros/qrcode?igrejaId=${igreja.id}` : `/api/igrejas/${igreja.id}/qrcode`;

  return (
    <main className="pagina-cartaz">
      <nav className="acoes-cartaz" aria-label="Ações do cartaz">
        <a href={enderecoRetorno}>← Voltar ao painel</a>
        <a
          className="botao-pdf-cartaz"
          href={enderecoPdf}
          target="_blank"
          rel="noopener"
        >Abrir PDF A4</a>
        <a className="botao-pdf-cartaz" href={`${enderecoPdf}${paraMembros ? "&" : "?"}quatroPorFolha=true`}
          target="_blank" rel="noopener">4 convites por A4</a>
      </nav>
      <p className="orientacao-pdf-cartaz">Para imprimir ou salvar, abra o PDF A4. Ele é gerado no servidor e não depende do tamanho da tela.</p>

      <article className={`cartaz-a4${paraMembros ? " convite-membros" : ""}`} aria-label={`Convite ${paraMembros ? "de cadastro de membros" : "de boas-vindas"} da ${igreja.nome}`}>
        <header className="cabecalho-cartaz">
          <LogoIgreja className="logo-cartaz" />
          <div>
            <span>{paraMembros ? "Cadastro de membros" : "Seja bem-vindo à"}</span>
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
          <span>{paraMembros ? "Nossa comunidade, nossos laços" : "Você é nosso convidado"}</span>
          <strong>Ainda há lugar.</strong>
          <cite>Lucas 14:22</cite>
        </section>

        <section className="conteudo-cartaz">
          <div className="texto-cartaz">
            <p className="rotulo-cartaz">{paraMembros ? "Cadastro de membros" : "Vamos manter contato?"}</p>
            <h1>{paraMembros ? "Uma família no amor de Deus" : "Queremos continuar essa conversa."}</h1>
            <p className="descricao-cartaz">
              {paraMembros ? "Escaneie o QR Code e informe seus dados para o cadastro de membros da igreja. Seu envio será analisado pela equipe pastoral." :
                "Escaneie o QR Code e deixe seu nome e WhatsApp. Nossa equipe entrará em contato para acolher você, apresentar melhor a igreja ou conversar e orar."}
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
                src={enderecoQr}
                alt={`QR Code para acessar o formulário de ${paraMembros ? "membros" : "visitantes"}`}
              />
            </div>
            <strong>Escaneie aqui</strong>
            <small>{paraMembros ? "Exclusivo para membros" : "É rápido e seguro"}</small>
          </div>
        </section>

        <footer className="rodape-cartaz">
          <p><strong>{paraMembros ? "Uma família no amor de Deus" : "Ficamos felizes com a sua visita."}</strong>
            <span>{paraMembros ? "Romanos 12:4-5" : "Esperamos falar com você em breve."}</span></p>
          <span className="endereco-rodape-cartaz">
            {formatarEndereco(igreja).split(" · CEP ")[0]}
            {igreja.cep && <> · <span className="cep-cartaz">CEP {formatarCep(igreja.cep)}</span></>}
          </span>
        </footer>
      </article>
    </main>
  );
}

function EstadoCartaz({ titulo, texto, enderecoRetorno }: { titulo: string; texto?: string; enderecoRetorno: string }) {
  return (
    <main className="pagina-mensagem">
      <section className="mensagem">
        <LogoIgreja />
        <h1>{titulo}</h1>
        {texto && <p>{texto}</p>}
        <a className="link-acao" href={enderecoRetorno}>Voltar ao painel</a>
      </section>
    </main>
  );
}
