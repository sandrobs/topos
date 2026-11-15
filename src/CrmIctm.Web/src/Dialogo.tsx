import { useEffect, useRef, type ReactNode } from "react";

/**
 * Sobreposição modal sobre `<dialog>` nativo, o mesmo padrão já usado em
 * `DialogoAvisoPrivacidade`. O elemento nativo entrega, sem código:
 * fechar com Escape, prender o foco dentro do diálogo, devolver o foco ao
 * gatilho ao fechar, tornar o resto da página inerte e travar a rolagem do
 * fundo. As sobreposições feitas à mão com `<div>` não faziam nada disso.
 *
 * Informe `rotuladoPor` com o id do título visível ou, na falta dele, `rotulo`.
 */
export function Dialogo({
  aoFechar,
  rotuladoPor,
  rotulo,
  descritoPor,
  centralizado = false,
  className = "",
  children,
}: {
  aoFechar: () => void;
  rotuladoPor?: string;
  rotulo?: string;
  descritoPor?: string;
  centralizado?: boolean;
  className?: string;
  children: ReactNode;
}) {
  const referencia = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialogo = referencia.current;
    const gatilho = document.activeElement;
    if (dialogo && !dialogo.open) dialogo.showModal();

    /*
     * O <dialog> devolve o foco ao fechar, mas aqui quem fecha é o React:
     * `aoFechar` muda o estado e o elemento é desmontado antes de o navegador
     * restituir o foco, que então cai no <body>. Fazemos isso na limpeza.
     * Se o gatilho saiu da página junto com a lista, não há o que restituir.
     */
    return () => {
      if (gatilho instanceof HTMLElement && gatilho.isConnected) gatilho.focus();
    };
  }, []);

  const classes = ["sobreposicao", centralizado ? "sobreposicao-dialogo" : "", className]
    .filter(Boolean)
    .join(" ");

  return (
    <dialog
      ref={referencia}
      className={classes}
      aria-labelledby={rotuladoPor}
      aria-label={rotuladoPor ? undefined : rotulo}
      aria-describedby={descritoPor}
      onCancel={aoFechar}
      onClose={aoFechar}
      onMouseDown={(evento) => {
        if (evento.target === evento.currentTarget) aoFechar();
      }}
    >
      {children}
    </dialog>
  );
}

/**
 * Confirmação de ação, no lugar de `window.confirm`. O diálogo nativo do
 * navegador trava a aba inteira, não é estilizável, aparece no idioma do
 * navegador e some do histórico do leitor de tela assim que fecha.
 */
export function DialogoConfirmacao({
  titulo, descricao, rotuloConfirmar, ocupado = false, aoConfirmar, aoCancelar,
}: {
  titulo: string;
  descricao: string;
  rotuloConfirmar: string;
  ocupado?: boolean;
  aoConfirmar: () => void;
  aoCancelar: () => void;
}) {
  return (
    <Dialogo centralizado rotuladoPor="titulo-confirmacao" descritoPor="descricao-confirmacao"
      aoFechar={aoCancelar}>
      <section className="dialogo-conclusao">
        <h2 id="titulo-confirmacao">{titulo}</h2>
        <p id="descricao-confirmacao">{descricao}</p>
        <div className="acoes-dialogo">
          <button type="button" className="botao-secundario" onClick={aoCancelar}>Cancelar</button>
          {/* Cancelar vem antes e recebe o foco inicial do <dialog>: numa ação
              destrutiva, o padrão seguro é não confirmar sem querer. */}
          <button type="button" disabled={ocupado} onClick={aoConfirmar}>
            {ocupado ? "Aguarde…" : rotuloConfirmar}
          </button>
        </div>
      </section>
    </Dialogo>
  );
}
