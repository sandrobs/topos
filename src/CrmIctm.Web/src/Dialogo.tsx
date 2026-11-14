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
