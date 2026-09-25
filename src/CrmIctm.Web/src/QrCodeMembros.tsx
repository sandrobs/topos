import { useEffect, useState } from "react";
import { listarIgrejas, type IgrejaInterna } from "./api";
import { enderecoPainelDaIgreja, salvarIgrejaSelecionada } from "./contextoIgreja";
import { formatarEndereco } from "./formatadores";
import { LogoIgreja } from "./LogoIgreja";

export function PaginaQrCodeMembros({ igrejaId }: { igrejaId: string }) {
  const [igreja, setIgreja] = useState<IgrejaInterna | null>(null);
  const [erro, setErro] = useState("");

  useEffect(() => {
    salvarIgrejaSelecionada(igrejaId);
    void listarIgrejas().then((itens) => {
      const selecionada = itens.find((item) => item.id === igrejaId && item.ativa);
      if (!selecionada) setErro("Igreja não encontrada ou sem acesso.");
      else setIgreja(selecionada);
    }).catch(() => setErro("Não foi possível preparar o QR Code."));
  }, [igrejaId]);

  if (!igreja) return <main className="pagina-mensagem"><section className="mensagem">
    <LogoIgreja /><h1>{erro || "Preparando QR Code…"}</h1>
    <a href={enderecoPainelDaIgreja(igrejaId)}>Voltar ao painel</a>
  </section></main>;

  return <main className="pagina-cartaz pagina-cartaz-membros">
    <nav className="acoes-cartaz">
      <a href={enderecoPainelDaIgreja(igreja.id)}>← Voltar ao painel</a>
      <button type="button" onClick={() => window.print()}>Imprimir em A4</button>
    </nav>
    <article className="cartaz-a4 cartaz-a4-membros">
      <header className="cabecalho-cartaz"><LogoIgreja className="logo-cartaz" />
        <div><span>Cadastro de membros</span><strong>{igreja.nome}</strong>
          <small>{igreja.cidade} · {igreja.estado}</small></div>
      </header>
      <section className="conteudo-cartaz-membros">
        <p className="rotulo-cartaz">Nossa comunidade</p>
        <h1>Você faz parte desta história.</h1>
        <p>Escaneie o QR Code para informar ou atualizar seus dados com a igreja.</p>
        <div className="moldura-qr-code"><img
          src={`/api/membros/qrcode?igrejaId=${encodeURIComponent(igreja.id)}`}
          alt="QR Code do cadastro de membros" /></div>
        <strong>Abra a câmera do celular e aponte para o código.</strong>
        <small>Seu envio será analisado pela equipe pastoral.</small>
      </section>
      <footer className="rodape-cartaz"><p><strong>{igreja.nome}</strong>
        <span>Um lugar para servir e pertencer.</span></p>
        <span className="nome-rodape-cartaz"><small>{formatarEndereco(igreja)}</small></span>
      </footer>
    </article>
  </main>;
}
