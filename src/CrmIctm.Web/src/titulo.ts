import { useEffect } from "react";

const MARCA = "Tópos - Gestão de Igrejas";

/**
 * Define o título da aba conforme a tela.
 *
 * Todas as rotas se chamavam "Igreja Evangélica de Cristo", o que atrapalha
 * quem trabalha com várias abas abertas — e o painel abre o cartaz numa aba
 * nova, então isso acontece no uso normal.
 *
 * O formulário público leva o nome da igreja na frente: é o que o visitante
 * reconhece. As telas internas levam a marca.
 */
export function useTitulo(titulo: string | null) {
  useEffect(() => {
    if (!titulo) return;
    document.title = titulo;
  }, [titulo]);
}

export function tituloInterno(secao: string) {
  return `${secao} · ${MARCA}`;
}

export function tituloPublico(pagina: string, nomeIgreja?: string | null) {
  return nomeIgreja ? `${pagina} · ${nomeIgreja}` : pagina;
}
