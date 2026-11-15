import type { ReactNode } from "react";
import { soltarFocoDePonteiro } from "./foco";
import type { PerfilUsuario } from "./api";

export type SecaoPainel =
  | "ATENDIMENTOS"
  | "ANIVERSARIANTES"
  | "MEMBROS"
  | "IGREJAS"
  | "USUARIOS";

/*
 * Os ícones só aparecem na barra inferior do celular. São SVG em linha, como
 * o do botão de sair: nenhum arquivo novo, nenhuma biblioteca.
 */
const traco = {
  viewBox: "0 0 24 24",
  fill: "none",
  stroke: "currentColor",
  strokeWidth: 2,
  strokeLinecap: "round" as const,
  strokeLinejoin: "round" as const,
  "aria-hidden": true,
  focusable: "false" as const,
};

const icones: Record<SecaoPainel, ReactNode> = {
  ATENDIMENTOS: (
    <svg className="icone-secao" {...traco}><path d="M3 12h4l2 6 4-14 2 8h6" /></svg>
  ),
  ANIVERSARIANTES: (
    <svg className="icone-secao" {...traco}>
      <path d="M4 20v-7h16v7z" /><path d="M3 20h18" />
      <path d="M8 13V9" /><path d="M12 13V9" /><path d="M16 13V9" />
      <path d="M11 6.6c0-1 1-1.4 1-2.4 0 1 1 1.4 1 2.4" />
    </svg>
  ),
  MEMBROS: (
    <svg className="icone-secao" {...traco}>
      <circle cx="9" cy="8" r="3.2" /><path d="M3 20a6 6 0 0 1 12 0" />
      <path d="M16 11a3 3 0 1 0 0-6" /><path d="M18 20a5.5 5.5 0 0 0-3-4.9" />
    </svg>
  ),
  IGREJAS: (
    <svg className="icone-secao" {...traco}>
      <path d="M3 11l9-8 9 8" /><path d="M5 10v10h14V10" /><path d="M10 20v-5h4v5" />
    </svg>
  ),
  USUARIOS: (
    <svg className="icone-secao" {...traco}>
      <circle cx="12" cy="8" r="3.4" /><path d="M5 20a7 7 0 0 1 14 0" />
    </svg>
  ),
};

/**
 * Seções do painel.
 *
 * `curto` é o rótulo da barra inferior do celular, onde cada posição tem
 * pouco mais de 70 px. `perfis` vazio significa "todos os perfis".
 *
 * Acrescentar uma seção é acrescentar uma linha aqui.
 *
 * São cinco, o limite combinado para a barra do celular: a 390 px cada posição
 * fica com 78 px, que ainda cabe o ícone e o rótulo curto. **A sexta seção
 * obriga a construir o "Mais"** — as de uso diário ficam na barra e as de
 * consulta ocasional vão para lá, agrupadas por assunto.
 */
const secoes: Array<{
  id: SecaoPainel;
  rotulo: string;
  curto: string;
  perfis: PerfilUsuario[];
}> = [
  { id: "ATENDIMENTOS", rotulo: "Atendimentos", curto: "Atend.", perfis: [] },
  { id: "ANIVERSARIANTES", rotulo: "Aniversariantes", curto: "Aniver.",
    perfis: ["ADMINISTRADOR", "PASTOR"] },
  { id: "MEMBROS", rotulo: "Membros", curto: "Membros", perfis: ["ADMINISTRADOR", "PASTOR"] },
  { id: "IGREJAS", rotulo: "Igrejas", curto: "Igrejas", perfis: ["ADMINISTRADOR"] },
  { id: "USUARIOS", rotulo: "Usuários", curto: "Usuários", perfis: ["ADMINISTRADOR", "PASTOR"] },
];

/** Vale para ler a seção pedida no endereço sem repetir a lista aqui e lá. */
export function ehSecaoDoPainel(valor: string | null): valor is SecaoPainel {
  return secoes.some((item) => item.id === valor);
}

export function NavegacaoPainel({
  secao, perfil, aniversariosHoje, aoTrocar,
}: {
  secao: SecaoPainel;
  perfil: PerfilUsuario | undefined;
  /** Quantos fazem aniversário hoje. Acima de zero, o menu recebe o sinal. */
  aniversariosHoje: number;
  aoTrocar: (secao: SecaoPainel) => void;
}) {
  const visiveis = secoes.filter(
    (item) => item.perfis.length === 0 || (perfil && item.perfis.includes(perfil)),
  );

  return (
    <nav className="navegacao-interna" aria-label="Seções do sistema">
      <div className="faixa-painel">
        {visiveis.map((item) => {
          const sinalizar = item.id === "ANIVERSARIANTES" && aniversariosHoje > 0;
          return (
            <button
              key={item.id}
              type="button"
              className={secao === item.id ? "ativo" : ""}
              /*
               * O rótulo curto é abreviado na barra do celular; o nome falado
               * continua inteiro. E o ponto de aniversário entra aqui também,
               * como número: a cor não pode ser o único jeito de saber que há
               * alguém para celebrar hoje.
               */
              aria-label={sinalizar ? `${item.rotulo}, ${aniversariosHoje} hoje` : item.rotulo}
              aria-current={secao === item.id ? "page" : undefined}
              onClick={(evento) => { aoTrocar(item.id); soltarFocoDePonteiro(evento); }}
            >
              {sinalizar && <span className="sinal-hoje" aria-hidden="true" />}
              {icones[item.id]}
              <span className="rotulo-secao">{item.rotulo}</span>
              <span className="rotulo-secao-curto">{item.curto}</span>
            </button>
          );
        })}
      </div>
    </nav>
  );
}
